using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

/// <summary>
///     Used to build the producer configuration
/// </summary>
public interface IProducerConfigurationBuilder
{
    /// <summary>
    ///     Gets the dependency injection configurator
    /// </summary>
    IServiceCollection ServiceCollection { get; }

    /// <summary>
    ///     Sets the default topic to be used when producing messages
    /// </summary>
    /// <param name="topic">Topic name</param>
    /// <returns></returns>
    IProducerConfigurationBuilder DefaultTopic(string topic);

    /// <summary>
    ///     Sets the <see cref="Acks" /> to be used when producing messages
    /// </summary>
    /// <param name="acks">The <see cref="Acks" /> enum value</param>
    /// <returns></returns>
    IProducerConfigurationBuilder WithAcks(Acks acks);

    /// <summary>
    ///     Delay in milliseconds to wait for messages in the producer queue to accumulate before constructing message batches
    ///     to transmit to brokers.
    ///     A higher value allows larger and more effective (less overhead, improved compression) batches of messages to
    ///     accumulate at the expense of increased message delivery latency.
    ///     default: 0.5 (500 microseconds)
    ///     importance: high
    /// </summary>
    /// <param name="lingerMs">The time in milliseconds to wait to build the message batch</param>
    /// <returns></returns>
    IProducerConfigurationBuilder WithLingerMs(double? lingerMs);

    /// <summary>
    ///     When set to 'true', the producer will ensure that exactly one copy of each message is written in the stream.
    ///     If 'false', producer retries due to broker failures, etc., may write duplicates of the retried message in the stream.
    ///     Note that enabling idempotence requires max.in.flight.requests.per.connection to be less than or equal to 5,
    ///     retries to be greater than 0 and acks must be 'all'. If these values are not explicitly set by the user,
    ///     suitable values will be chosen. If incompatible values are set, a ConfigException will be thrown.
    /// </summary>
    /// <param name="idempotence">Gets ordering and durability</param>
    /// <returns></returns>
    /// 
    IProducerConfigurationBuilder WithIdempotence(bool? idempotence);
        
    /// <summary>
    /// How many times to retry sending a failing Message. Note: retrying may cause reordering unless enable.idempotence is set to true.
    /// default: 2147483647
    /// </summary>
    /// <param name="maxRetries"></param>
    /// <returns></returns>
    IProducerConfigurationBuilder WithMaxRetries(int? maxRetries);

    /// <summary>
    /// The backoff time in milliseconds before retrying a protocol request.
    ///default: 100
    /// </summary>
    /// <param name="retryBackoffMs"></param>
    /// <returns></returns>
    IProducerConfigurationBuilder WithRetryBackoffMs(int? retryBackoffMs);
        
    /// <summary>
    ///     Adds a handler for the Kafka producer statistics
    /// </summary>
    /// <param name="statisticsHandler">A handler for the statistics</param>
    /// <returns></returns>
    IProducerConfigurationBuilder WithStatisticsHandler(Action<string> statisticsHandler);

    /// <summary>
    ///     Sets the interval the statistics are emitted
    /// </summary>
    /// <param name="statisticsIntervalMs">The interval in milliseconds</param>
    /// <returns></returns>
    IProducerConfigurationBuilder WithStatisticsIntervalMs(int? statisticsIntervalMs);
}