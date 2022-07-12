using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

internal interface IConsumerWorkerPool
{
    Task Start(IEnumerable<TopicPartition> partitions);

    Task Stop();

    Task Enqueue(
        ConsumeResult<byte[], byte[]> message,
        CancellationToken stopCancellationToken);
}