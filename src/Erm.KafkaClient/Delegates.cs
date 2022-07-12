using System;
using Confluent.Kafka;
using Erm.KafkaClient.Consumers;

namespace Erm.KafkaClient;

/// <summary>
///     A factory to decorates the consumer created by Erm.KafkaClient
/// </summary>
/// <param name="consumer">The consumer created by Erm.KafkaClient</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider" /> to get registered services</param>
public delegate IConsumer ConsumerFactory(
    IConsumer consumer,
    IServiceProvider serviceProvider);

/// <summary>
///     A factory to decorates the producer created by Erm.KafkaClient
/// </summary>
/// <param name="producer">The producer created by Erm.KafkaClient</param>
/// <param name="serviceProvider">The <see cref="IServiceProvider" /> to get registered services</param>
public delegate IProducer<byte[], byte[]> ProducerFactory(
    IProducer<byte[], byte[]> producer,
    IServiceProvider serviceProvider);
    
    
/// <summary>
///     Defines a factory to create an instance of <typeparamref name="T" /> type
/// </summary>
/// <param name="serviceProvider">A <see cref="IServiceProvider" /> instance</param>
public delegate T Factory<out T>(IServiceProvider serviceProvider);