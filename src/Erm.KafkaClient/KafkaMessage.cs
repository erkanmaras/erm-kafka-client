namespace Erm.KafkaClient;

/// <summary>
///     Represents a Kafka message
/// </summary>
public readonly struct KafkaMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KafkaMessage" /> struct.
    /// </summary>
    /// <param name="key">
    ///     <inheritdoc cref="Key" />
    /// </param>
    /// <param name="value">
    ///     <inheritdoc cref="Value" />
    /// </param>
    public KafkaMessage(byte[] key, byte[] value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    ///     Gets the message key
    /// </summary>
    public byte[] Key { get; }

    /// <summary>
    ///     Gets the message value
    /// </summary>
    public byte[] Value { get; }
}