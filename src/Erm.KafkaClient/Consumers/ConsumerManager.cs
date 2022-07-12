using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class ConsumerManager : IConsumerManager
{
    private readonly ILogger<ConsumerManager> _logger;

    private CancellationTokenSource _stopCancellationTokenSource;

    public ConsumerManager(
        IConsumer consumer,
        IConsumerWorkerPool consumerWorkerPool,
        IWorkerPoolFeeder feeder,
        ILogger<ConsumerManager> logger)
    {
        _logger = logger;
        Consumer = consumer;
        WorkerPool = consumerWorkerPool;
        Feeder = feeder;

        Consumer.OnPartitionsAssigned((_, _, partitions) => OnPartitionAssigned(partitions));
        Consumer.OnPartitionsRevoked((_, _, partitions) => OnPartitionRevoked(partitions));
    }

    public IWorkerPoolFeeder Feeder { get; }

    public IConsumerWorkerPool WorkerPool { get; }

    public IConsumer Consumer { get; }

    public Task Start()
    {
        _stopCancellationTokenSource = new CancellationTokenSource();
        Feeder.Start();
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        if (_stopCancellationTokenSource is { Token: { CanBeCanceled: true } })
        {
            _stopCancellationTokenSource.Cancel();
        }

        await Feeder.Stop().ConfigureAwait(false);
        await WorkerPool.Stop().ConfigureAwait(false);

        Consumer.Dispose();
    }

    private void OnPartitionRevoked(IEnumerable<TopicPartitionOffset> topicPartitions)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("Partitions revoked {ConsumerInfo}", GetConsumerLogInfo(topicPartitions.Select(x => x.TopicPartition)));
        }
            
        WorkerPool.Stop().GetAwaiter().GetResult();
    }

    private void OnPartitionAssigned(IReadOnlyCollection<TopicPartition> partitions)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Partitions assigned {ConsumerInfo}", GetConsumerLogInfo(partitions));
        }
            
        WorkerPool.Start(partitions).GetAwaiter().GetResult();
    }

    private object GetConsumerLogInfo(IEnumerable<TopicPartition> partitions) => new
    {
        Consumer.Configuration.GroupId,
        Consumer.Configuration.ConsumerName,
        Topics = JsonSerializer.Serialize(partitions
            .GroupBy(x => x.Topic)
            .Select(
                x => new
                {
                    x.First().Topic,
                    PartitionsCount = x.Count(),
                    Partitions = x.Select(y => y.Partition.Value)
                }).ToList())
    };
}