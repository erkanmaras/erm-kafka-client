using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

internal interface IOffsetManager
{
    void MarkAsProcessed(TopicPartitionOffset offset);
}