using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Erm.KafkaClient.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

/// <summary>
///     Used to build the consumer configuration
/// </summary>
public interface IConsumerConfigurationBuilder
{
    /// <summary>
    ///     Gets the dependency injection configurator
    /// </summary>
    IServiceCollection ServiceCollection { get; }

    /// <summary>
    ///     Sets the topic that will be used to read the messages
    /// </summary>
    /// <param name="topicName">Topic name</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder Topic(string topicName);

    /// <summary>
    ///     Sets the topics that will be used to read the messages
    /// </summary>
    /// <param name="topicNames">Topic names</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder Topics(IEnumerable<string> topicNames);

    /// <summary>
    ///     Sets the topics that will be used to read the messages
    /// </summary>
    /// <param name="topicNames">Topic names</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder Topics(params string[] topicNames);

    /// <summary>
    ///     Sets a unique name for the consumer
    /// </summary>
    /// <param name="name">A unique name</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithName(string name);

    /// <summary>
    ///     Sets the group id used by the consumer
    /// </summary>
    /// <param name="groupId">The consumer id value</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithGroupId(string groupId);

    /// <summary>
    ///     Sets the initial offset strategy used by new consumer groups.
    ///     If your consumer group (set by method <see cref="WithGroupId(string)" />) has no offset stored in Kafka, this
    ///     configuration will be used
    /// </summary>
    /// <param name="autoOffsetReset">The <see cref="AutoOffsetReset" /> enum value</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithAutoOffsetReset(AutoOffsetReset autoOffsetReset);

    /// <summary>
    ///     Sets the interval used by the framework to commit the stored offsets in Kafka
    /// </summary>
    /// <param name="autoCommitIntervalMs">The interval in milliseconds</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithAutoCommitIntervalMs(int autoCommitIntervalMs);

    /// <summary>
    ///     Sets the max interval between message consumption, if this time exceeds the consumer is considered failed and Kafka
    ///     will revoke the assigned partitions
    /// </summary>
    /// <param name="maxPollIntervalMs">The interval in milliseconds</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithMaxPollIntervalMs(int? maxPollIntervalMs);

    /// <summary>
    ///     The timeout used to detect client failures when using Kafka's group management facility.
    ///     The client sends periodic heartbeats to indicate its liveness to the broker.
    ///     If no heartbeats are received by the broker before the expiration of this session timeout, then the broker will remove this client
    ///     from the group and initiate a rebalance.
    /// </summary>
    /// <param name="sessionTimeoutMs">The interval in milliseconds</param>
    /// <returns></returns>
    public IConsumerConfigurationBuilder WithSessionTimeoutMs(int? sessionTimeoutMs);

    /// <summary>
    ///    The expected time between heartbeats to the consumer coordinator when using Kafka's group management facilities.
    ///    Heartbeats are used to ensure that the consumer's session stays active and to facilitate rebalancing when new consumers
    ///    join or leave the group. The value must be set lower than session.timeout.ms, but typically should be set no higher than 1/3 of that value.
    ///    It can be adjusted even lower to control the expected time for normal rebalances.
    /// </summary>
    /// <param name="heartbeatIntervalMs">The interval in milliseconds</param>
    /// <returns></returns>
    public IConsumerConfigurationBuilder WithHeartbeatIntervalMs(int? heartbeatIntervalMs);
        
    /// <summary>
    ///     Sets the number of threads that will be used to consume the messages
    /// </summary>
    /// <param name="workersCount">The number of workers</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithWorkersCount(int workersCount);

    /// <summary>
    ///     Sets how many messages will be buffered for each worker
    /// </summary>
    /// <param name="size">The buffer size</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithBufferSize(int size);

    /// <summary>
    ///     Sets the strategy to choose a worker when a message arrives
    /// </summary>
    /// <typeparam name="T">A class that implements the <see cref="IDistributionStrategy" /> interface</typeparam>
    /// <param name="factory">A factory to create the instance</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithWorkDistributionStrategy<T>(Factory<T> factory)
        where T : class, IDistributionStrategy;

    /// <summary>
    ///     Sets the strategy to choose a worker when a message arrives
    /// </summary>
    /// <typeparam name="T">A class that implements the <see cref="IDistributionStrategy" /> interface</typeparam>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithWorkDistributionStrategy<T>()
        where T : class, IDistributionStrategy;

    /// <summary>
    ///     Offsets will be stored after the execution of the handler and middlewares automatically, this is the default
    ///     behaviour
    /// </summary>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithAutoStoreOffsets();

    /// <summary>
    ///     The client should call the <see cref="IConsumerContext.StoreOffset()" />
    /// </summary>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithManualStoreOffsets();
        
    /// <summary>
    ///     Register the message handler for this consumer
    /// </summary>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithHandler<THandler>() where  THandler : class, IConsumerMessageHandler;
        
    /// <summary>
    ///     Adds a handler for the Kafka consumer statistics
    /// </summary>
    /// <param name="statisticsHandler">A handler for the statistics</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithStatisticsHandler(Action<string> statisticsHandler);

    /// <summary>
    ///     Sets the interval the statistics are emitted
    /// </summary>
    /// <param name="statisticsIntervalMs">The interval in milliseconds</param>
    /// <returns></returns>
    IConsumerConfigurationBuilder WithStatisticsIntervalMs(int statisticsIntervalMs);
}