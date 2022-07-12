using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Erm.KafkaClient.Producers;

internal class TypedKafkaProducer<TProducer> : IKafkaProducer<TProducer>
{
    private readonly IKafkaProducer _kafkaProducer;

    public TypedKafkaProducer(IKafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public string ProducerName => _kafkaProducer.ProducerName;

    public Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
        string topic,
        byte[] messageKey,
        byte[] message,
        IKafkaMessageHeaders headers = null)
    {
        return _kafkaProducer.ProduceAsync(
            topic,
            messageKey,
            message,
            headers);
    }

    public Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
        byte[] partitionKey,
        byte[] message,
        IKafkaMessageHeaders headers = null)
    {
        return _kafkaProducer.ProduceAsync(
            partitionKey,
            message,
            headers);
    }

    public void Produce(
        string topic,
        byte[] partitionKey,
        byte[] message,
        IKafkaMessageHeaders headers = null,
        Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null)
    {
        _kafkaProducer.Produce(
            topic,
            partitionKey,
            message,
            headers,
            deliveryHandler);
    }

    public void Produce(
        byte[] partitionKey,
        byte[] message,
        IKafkaMessageHeaders headers = null,
        Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null)
    {
        _kafkaProducer.Produce(
            partitionKey,
            message,
            headers,
            deliveryHandler);
    }
}