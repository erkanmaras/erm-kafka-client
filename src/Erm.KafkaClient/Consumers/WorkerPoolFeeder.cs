using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class WorkerPoolFeeder : IWorkerPoolFeeder
{
    private readonly IConsumer _consumer;
    private readonly ILogger _logger;
    private readonly IConsumerWorkerPool _workerPool;
    private Task _feederTask;

    private CancellationTokenSource _stopTokenSource;

    public WorkerPoolFeeder(
        IConsumer consumer,
        IConsumerWorkerPool workerPool,
        ILogger logger)
    {
        _consumer = consumer;
        _workerPool = workerPool;
        _logger = logger;
    }

    public void Start()
    {
        _stopTokenSource = new CancellationTokenSource();
        var token = _stopTokenSource.Token;

        _feederTask = Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested)
                    try
                    {
                        var message = await _consumer.ConsumeAsync(token).ConfigureAwait(false);
                        await _workerPool.Enqueue(message, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Do nothing
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka");
                    }
            },
            CancellationToken.None);
    }

    public Task Stop()
    {
        if (_stopTokenSource is { IsCancellationRequested: false })
        {
            _stopTokenSource.Cancel();
            _stopTokenSource.Dispose();
        }

        return _feederTask ?? Task.CompletedTask;
    }
}