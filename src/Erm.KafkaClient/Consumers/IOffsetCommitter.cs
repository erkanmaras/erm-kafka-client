using System;
using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

internal interface IOffsetCommitter : IDisposable
{
    void StoreOffset(TopicPartitionOffset tpo);
}