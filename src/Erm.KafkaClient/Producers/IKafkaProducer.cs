using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Erm.KafkaClient.Producers;

/// <summary>
///     Provides access to the kafka message producer
/// </summary>
/// <typeparam name="TProducer">The producer associated type</typeparam>
// ReSharper disable once UnusedTypeParameter
public interface IKafkaProducer<TProducer> : IKafkaProducer
{
}

/// <summary>
///     Provides access to the kafka producer
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    ///     Gets the unique producer's name defined in the configuration
    /// </summary>
    string ProducerName { get; }

    /// <summary>
    ///     Produces a new message
    /// </summary>
    /// <param name="topic">The topic where the message wil be produced</param>
    /// <param name="messageKey">The message key</param>
    /// <param name="messageValue">The message value</param>
    /// <param name="headers">The message headers</param>
    /// <returns></returns>
    Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
        string topic,
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null);

    /// <summary>
    ///     Produces a new message in the configured default topic
    /// </summary>
    /// <param name="messageKey">The message key</param>
    /// <param name="messageValue">The message value</param>
    /// <param name="headers">The message headers</param>
    /// <returns></returns>
    Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null);

    /// <summary>
    ///     Produces a new message
    ///     This should be used for high throughput scenarios:
    ///     <see href="https://github.com/confluentinc/confluent-kafka-dotnet/wiki/Producer#produceasync-vs-produce" />
    /// </summary>
    /// <param name="topic">The topic where the message wil be produced</param>
    /// <param name="messageKey">The message key</param>
    /// <param name="messageValue">The message value</param>
    /// <param name="headers">The message headers</param>
    /// <param name="deliveryHandler">A handler with the operation result</param>
    void Produce(
        string topic,
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null,
        Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null);

    /// <summary>
    ///     Produces a new message in the configured default topic
    ///     This should be used for high throughput scenarios:
    ///     <see href="https://github.com/confluentinc/confluent-kafka-dotnet/wiki/Producer#produceasync-vs-produce" />
    /// </summary>
    /// <param name="messageKey">The message key</param>
    /// <param name="messageValue">The message value</param>
    /// <param name="headers">The message headers</param>
    /// <param name="deliveryHandler">A handler with the operation result</param>
    void Produce(
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null,
        Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null);
}