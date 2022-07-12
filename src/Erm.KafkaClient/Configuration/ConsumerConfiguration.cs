using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Erm.KafkaClient.Configuration;

internal class ConsumerConfiguration : IConsumerConfiguration
{
    private readonly ConsumerConfig _consumerConfig;
    private int _workerCount;

    public ConsumerConfiguration(
        ConsumerConfig consumerConfig,
        string consumerName,
        IEnumerable<string> topics,
        int workerCount,
        int bufferSize,
        bool autoStoreOffsets,
        TimeSpan autoCommitInterval,
        Type consumerHandlerType,
        Action<string> statisticsHandler,
        Factory<IDistributionStrategy> distributionStrategyFactory,
        Action<IServiceProvider, List<TopicPartition>> partitionsAssignedHandler,
        Action<IServiceProvider, List<TopicPartitionOffset>> partitionsRevokedHandler,
        ConsumerFactory factory)
    {
        _consumerConfig = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));

        if (string.IsNullOrEmpty(_consumerConfig.GroupId))
        {
            throw new ArgumentNullException(nameof(consumerConfig.GroupId));
        }

        DistributionStrategyFactory = distributionStrategyFactory ?? throw new ArgumentNullException(nameof(distributionStrategyFactory));
        ConsumerHandlerType = consumerHandlerType;
        AutoStoreOffsets = autoStoreOffsets;
        AutoCommitInterval = autoCommitInterval;
        Topics = topics ?? throw new ArgumentNullException(nameof(topics));
        ConsumerName = consumerName ?? "Consumer_" + Guid.NewGuid().ToString("N");
        WorkerCount = workerCount;
        StatisticsHandler = statisticsHandler;
        PartitionsAssignedHandler = partitionsAssignedHandler;
        PartitionsRevokedHandler = partitionsRevokedHandler;
        Factory = factory;
        BufferSize = bufferSize > 0
            ? bufferSize
            : throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "The consumer bufferSize must be greater than 0");
    }

    public Factory<IDistributionStrategy> DistributionStrategyFactory { get; }

    public Type ConsumerHandlerType { get; }

    public IEnumerable<string> Topics { get; }

    public string ConsumerName { get; }

    public int WorkerCount
    {
        get => _workerCount;
        set =>
            _workerCount = value is > 0 and <= byte.MaxValue
                ? value
                : throw new ArgumentOutOfRangeException(
                    nameof(WorkerCount),
                    WorkerCount,
                    $"The consumer {nameof(WorkerCount)} value must be greater than 0 and lower or equal than 255!");
    }

    public string GroupId => _consumerConfig.GroupId;

    public int BufferSize { get; }

    public bool AutoStoreOffsets { get; }

    public TimeSpan AutoCommitInterval { get; }

    public Action<string> StatisticsHandler { get; }

    public Action<IServiceProvider, List<TopicPartition>> PartitionsAssignedHandler { get; }

    public Action<IServiceProvider, List<TopicPartitionOffset>> PartitionsRevokedHandler { get; }

    public ConsumerFactory Factory { get; }

    public ConsumerConfig GetKafkaConfig()
    {
        return _consumerConfig;
    }
}