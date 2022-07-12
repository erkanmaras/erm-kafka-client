using System.Collections.Generic;

namespace Erm.KafkaClient.Producers;

/// <summary>
///     Provides access to the configured producers
/// </summary>
public interface IProducerAccessor
{
    /// <summary>
    ///     Gets all configured producers
    /// </summary>
    IEnumerable<IKafkaProducer> All { get; }

    /// <summary>
    ///     Gets a producer by its name
    /// </summary>
    /// <param name="name">producer name</param>
    IKafkaProducer this[string name] { get; }

    /// <summary>
    ///     Gets a producer by its name
    /// </summary>
    /// <param name="name">The name defined in the producer configuration</param>
    /// <returns></returns>
    IKafkaProducer GetProducer(string name);

    /// <summary>
    ///     Gets a producer by its type
    /// </summary>
    /// <typeparam name="TProducer">The type defined in the configuration</typeparam>
    /// <returns></returns>
    IKafkaProducer GetProducer<TProducer>();
}