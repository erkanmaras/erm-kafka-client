using System.Collections.Generic;

namespace Erm.KafkaClient.Consumers;

/// <summary>
///     Provides access to the configured consumers
/// </summary>
public interface IConsumerAccessor
{
    /// <summary>
    ///     Gets all configured consumers
    /// </summary>
    IEnumerable<IKafkaConsumer> All { get; }

    /// <summary>
    ///     Gets a consumer by its name
    /// </summary>
    /// <param name="name">consumer name</param>
    IKafkaConsumer this[string name] { get; }

    /// <summary>
    ///     Gets a consumer by its name
    /// </summary>
    /// <param name="name">The name defined in the consumer configuration</param>
    /// <returns></returns>
    IKafkaConsumer GetConsumer(string name);

    internal void Add(IKafkaConsumer kafkaConsumer);
}