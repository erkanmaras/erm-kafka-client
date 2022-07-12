using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

/// <summary>
///     Provides access to the kafka message consumer
/// </summary>
public interface IKafkaConsumer
{
    /// <summary>
    ///     Gets the unique consumer´s name defined in the configuration
    /// </summary>
    string ConsumerName { get; }

    /// <summary>
    ///     Gets the group id define in the configuration
    /// </summary>
    string GroupId { get; }

    /// <summary>
    ///     Gets the current topic subscription
    /// </summary>
    IReadOnlyList<string> Subscription { get; }

    /// <summary>
    ///     Gets the current partition assignment
    /// </summary>
    IReadOnlyList<TopicPartition> Assignment { get; }

    /// <summary>
    ///     Gets the (dynamic) group member id of this consumer (as set by the broker).
    /// </summary>
    string MemberId { get; }

    /// <summary>
    ///     Gets the name of this client instance.
    ///     Contains (but is not equal to) the client.id
    ///     configuration parameter.
    /// </summary>
    /// <remarks>
    ///     This name will be unique across all client
    ///     instances in a given application which allows
    ///     log messages to be associated with the
    ///     corresponding instance.
    /// </remarks>
    string ClientInstanceName { get; }

    /// <summary>
    ///     Gets the current number of workers allocated of the consumer
    /// </summary>
    int WorkerCount { get; }
 
    /// <summary>
    ///     Overrides the offsets of the given partitions and restart the consumer
    /// </summary>
    /// <param name="offsets">The offset values</param>
    Task OverrideOffsetsAndRestart(IReadOnlyCollection<TopicPartitionOffset> offsets);
        
    /// <summary>
    ///     Restart Erm.KafkaClient consumer and recreate the internal Confluent Consumer
    /// </summary>
    /// <returns></returns>
    Task Restart();
        
    /// <summary>
    ///     Gets the current position (offset) for the
    ///     specified topic / partition.
    ///     The offset field of each requested partition
    ///     will be set to the offset of the last consumed
    ///     message + 1, or Offset.Unset in case there was
    ///     no previous message consumed by this consumer.
    /// </summary>
    /// <exception cref="T:Confluent.Kafka.KafkaException">
    ///     Thrown if the request failed.
    /// </exception>
    Offset GetPosition(TopicPartition topicPartition);
        
}