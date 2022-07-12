using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class ConsumerWorker : IConsumerWorker
{
    private readonly IConsumer _consumer;
    private readonly ILogger<ConsumerWorker> _logger;
    private readonly IConsumerMessageHandler _messageHandler;
    private readonly Channel<ConsumeResult<byte[], byte[]>> _messagesBuffer;
    private readonly IOffsetManager _offsetManager;
    private Task _backgroundTask;
    private Action _onMessageFinishedHandler;
    private CancellationTokenSource _stopCancellationTokenSource;

    public ConsumerWorker(
        IConsumer consumer,
        int workerId,
        IOffsetManager offsetManager,
        ILogger<ConsumerWorker> logger,
        IConsumerMessageHandler messageHandler)
    {
        Id = workerId;
        _consumer = consumer;
        _offsetManager = offsetManager;
        _logger = logger;
        _messageHandler = messageHandler;
        _messagesBuffer = Channel.CreateBounded<ConsumeResult<byte[], byte[]>>(new BoundedChannelOptions(consumer.Configuration.BufferSize)
        {
            SingleReader = true,
            SingleWriter = true,
        });
    }

    public int Id { get; }

    public ValueTask Enqueue(
        ConsumeResult<byte[], byte[]> message,
        CancellationToken stopCancellationToken = default)
    {
        return _messagesBuffer.Writer.WriteAsync(message, stopCancellationToken);
    }

    public Task Start()
    {
        _stopCancellationTokenSource = new CancellationTokenSource();

        _backgroundTask = Task.Run(
            async () =>
            {
                while (!_stopCancellationTokenSource.IsCancellationRequested)
                    try
                    {
                        var message = await _messagesBuffer.Reader.ReadAsync(_stopCancellationTokenSource.Token).ConfigureAwait(false);
                        var context = new KafkaMessageContext(
                            new KafkaMessage(message.Message.Key, message.Message.Value),
                            new KafkaMessageHeaders(message.Message.Headers),
                            new ConsumerContext(
                                _consumer,
                                _offsetManager,
                                message,
                                Id,
                                _stopCancellationTokenSource.Token), producer: null);

                        try
                        {
                            await _messageHandler.Handle(context).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                "Error executing consumer exception: {Exception} context: {@MessageContext}",
                                ex,
                                new
                                {
                                    context.ConsumerContext.ConsumerName,
                                    context.ConsumerContext.Topic,
                                    Message = context.KafkaMessage.Value,
                                    MessageKey = context.KafkaMessage.Value
                                });
                        }
                        finally
                        {
                            if (_consumer.Configuration.AutoStoreOffsets && context.ConsumerContext.ShouldStoreOffset)
                            {
                                _offsetManager.MarkAsProcessed(message.TopicPartitionOffset);
                            }

                            _onMessageFinishedHandler?.Invoke();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogTrace("Consume operation was cancelled!");
                    }
            });

        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        if (_stopCancellationTokenSource.Token.CanBeCanceled)
        {
            _stopCancellationTokenSource.Cancel();
        }

        await _backgroundTask.ConfigureAwait(false);
        _backgroundTask.Dispose();
    }

    public void OnTaskCompleted(Action handler)
    {
        _onMessageFinishedHandler = handler;
    }
}