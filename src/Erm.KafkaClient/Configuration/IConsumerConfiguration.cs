using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Erm.KafkaClient.Consumers;

namespace Erm.KafkaClient.Configuration;

/// <summary>
///     Represents the Consumer configuration values
/// </summary>
public interface IConsumerConfiguration
{
    /// <summary>
    ///     Gets the consumer worker distribution strategy
    /// </summary>
    Factory<IDistributionStrategy> DistributionStrategyFactory { get; }

    /// <summary>
    ///     Message handler
    /// </summary>
    Type ConsumerHandlerType { get; }

    /// <summary>
    ///     Gets the consumer configured topics
    /// </summary>
    IEnumerable<string> Topics { get; }

    /// <summary>
    ///     Gets the consumer name
    /// </summary>
    string ConsumerName { get; }

    /// <summary>
    ///     Gets or sets the current number of workers
    /// </summary>
    int WorkerCount { get; set; }

    /// <summary>
    ///     Gets the consumer group
    /// </summary>
    string GroupId { get; }

    /// <summary>
    ///     Gets the buffer size used for each worker
    /// </summary>
    int BufferSize { get; }

    /// <summary>
    ///     Gets a value indicating whether if the application should store store at the end
    /// </summary>
    bool AutoStoreOffsets { get; }

    /// <summary>
    ///     Gets the interval between commits
    /// </summary>
    TimeSpan AutoCommitInterval { get; }

    /// <summary>
    ///     Gets the handlers used to collects statistics
    /// </summary>
    Action<string> StatisticsHandler { get; }

    /// <summary>
    ///     Gets the handlers that will be called when the partitions are assigned
    /// </summary>
    Action<IServiceProvider, List<TopicPartition>> PartitionsAssignedHandler { get; }

    /// <summary>
    ///     Gets the handlers that will be called when the partitions are revoked
    /// </summary>
    Action<IServiceProvider, List<TopicPartitionOffset>> PartitionsRevokedHandler { get; }

    /// <summary>
    ///     Gets the custom factory used to create a new <see cref="IConsumer" />
    /// </summary>
    ConsumerFactory Factory { get; }

    /// <summary>
    ///    Gets the Confluent consumer configuration
    /// </summary>
    /// <returns></returns>
    ConsumerConfig GetKafkaConfig();
}