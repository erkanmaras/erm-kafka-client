using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Erm.KafkaClient.Configuration;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class Consumer : IConsumer
{
    private readonly List<Action<IConsumer<byte[], byte[]>, Error>> _errorsHandlers = new();
    private readonly ILogger<Consumer> _logger;
    private readonly List<Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartition>>> _partitionsAssignedHandlers = new();
    private readonly List<Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>>> _partitionsRevokedHandlers = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Action<IConsumer<byte[], byte[]>, string>> _statisticsHandlers = new();
    private IConsumer<byte[], byte[]> _consumer;

    public Consumer(
        IConsumerConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<Consumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        Configuration = configuration;

        if (Configuration.StatisticsHandler != null)
        {
            OnStatistics((_, statistics) => Configuration.StatisticsHandler(statistics));
        }

        if (Configuration.PartitionsAssignedHandler != null)
        {
            OnPartitionsAssigned((resolver, _, topicPartitions) => Configuration.PartitionsAssignedHandler(resolver, topicPartitions));
        }
           
        if (Configuration.PartitionsRevokedHandler != null)
        {
            OnPartitionsRevoked((resolver, _, topicPartitions) => Configuration.PartitionsRevokedHandler(resolver, topicPartitions));
        }
    }

    public IConsumerConfiguration Configuration { get; }

    public IReadOnlyList<string> Subscription => _consumer?.Subscription.AsReadOnly();

    public IReadOnlyList<TopicPartition> Assignment => _consumer?.Assignment.AsReadOnly();

    public string MemberId => _consumer?.MemberId;

    public string ClientInstanceName => _consumer?.Name;

    public void OnPartitionsAssigned(Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartition>> handler)
    {
        _partitionsAssignedHandlers.Add(handler);
    }

    public void OnPartitionsRevoked(Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>> handler)
    {
        _partitionsRevokedHandlers.Add(handler);
    }

    public void OnError(Action<IConsumer<byte[], byte[]>, Error> handler)
    {
        _errorsHandlers.Add(handler);
    }

    public void OnStatistics(Action<IConsumer<byte[], byte[]>, string> handler)
    {
        _statisticsHandlers.Add(handler);
    }

    public Offset GetPosition(TopicPartition topicPartition)
    {
        return _consumer.Position(topicPartition);
    }

    public void Commit(IEnumerable<TopicPartitionOffset> offsetsValues)
    {
        _consumer.Commit(offsetsValues);
    }

    public async ValueTask<ConsumeResult<byte[], byte[]>> ConsumeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                EnsureConsumer();

                return _consumer.Consume(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (KafkaException ex) when (ex.Error.IsFatal)
            {
                _logger.LogError(ex, "Kafka Consumer fatal error occurred. Recreating consumer in 5 seconds");

                InvalidateConsumer();

                await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka Consumer Error");
            }
        }
    }

    public void Dispose()
    {
        InvalidateConsumer();
    }

    public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition)
    {
        return _consumer.GetWatermarkOffsets(topicPartition);
    }

    public WatermarkOffsets QueryWatermarkOffsets(TopicPartition topicPartition, TimeSpan timeout)
    {
        return _consumer.QueryWatermarkOffsets(topicPartition, timeout);
    }

    public List<TopicPartitionOffset> OffsetsForTimes(IEnumerable<TopicPartitionTimestamp> topicPartitions, TimeSpan timeout)
    {
        return _consumer.OffsetsForTimes(topicPartitions, timeout);
    }

    private void EnsureConsumer()
    {
        if (_consumer != null)
        {
            return;
        }

        var kafkaConfig = Configuration.GetKafkaConfig();
        var consumerBuilder = new ConsumerBuilder<byte[], byte[]>(kafkaConfig);

        _consumer = consumerBuilder
            .SetPartitionsAssignedHandler((consumer, partitions) => _partitionsAssignedHandlers.ForEach(x => x(_serviceProvider, consumer, partitions)))
            .SetPartitionsRevokedHandler((consumer, partitions) => _partitionsRevokedHandlers.ForEach(x => x(_serviceProvider, consumer, partitions)))
            .SetErrorHandler((consumer, error) => _errorsHandlers.ForEach(x => x(consumer, error)))
            .SetStatisticsHandler((consumer, statistics) => _statisticsHandlers.ForEach(x => x(consumer, statistics)))
            .Build();

        _consumer.Subscribe(Configuration.Topics);
          
    }

    private void InvalidateConsumer()
    {
        _consumer?.Close();
        _consumer = null;
    }
}