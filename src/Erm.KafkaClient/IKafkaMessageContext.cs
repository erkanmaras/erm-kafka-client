using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;

namespace Erm.KafkaClient;

/// <summary>
///     A context that contains the message and metadata
/// </summary>
public interface IKafkaMessageContext
{
    /// <summary>
    ///     Gets the message
    /// </summary>
    KafkaMessage KafkaMessage { get; }

    /// <summary>
    ///     Gets the message headers
    /// </summary>
    IKafkaMessageHeaders Headers { get; }

    /// <summary>
    ///     Gets the <see cref="IConsumerContext"></see> from the consumed message
    /// </summary>
    IConsumerContext ConsumerContext { get; }

    /// <summary>
    ///     Gets the <see cref="IProducerContext"></see> from the produced message
    /// </summary>
    IProducerContext ProducerContext { get; }

    /// <summary>
    ///     Creates a new <see cref="IKafkaMessageContext" /> with the new message
    /// </summary>
    /// <param name="key">The new message key</param>
    /// <param name="value">The new message value</param>
    /// <returns>A new message context containing the new values</returns>
    IKafkaMessageContext SetMessage(byte[] key, byte[] value);
}