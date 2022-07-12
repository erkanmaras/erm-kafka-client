using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;

namespace Erm.KafkaClient;

internal class KafkaMessageContext : IKafkaMessageContext
{
    public KafkaMessageContext(
        KafkaMessage kafkaMessage,
        IKafkaMessageHeaders headers,
        IConsumerContext consumer,
        IProducerContext producer)
    {
        KafkaMessage = kafkaMessage;
        Headers = headers ?? new KafkaMessageHeaders();
        ConsumerContext = consumer;
        ProducerContext = producer;
    }

    public KafkaMessage KafkaMessage { get; }
    public IConsumerContext ConsumerContext { get; }
    public IProducerContext ProducerContext { get; }
    public IKafkaMessageHeaders Headers { get; }

    public IKafkaMessageContext SetMessage(byte[] key, byte[] value)
    {
        return new KafkaMessageContext(
            new KafkaMessage(key, value),
            Headers,
            ConsumerContext,
            ProducerContext);
    }
}