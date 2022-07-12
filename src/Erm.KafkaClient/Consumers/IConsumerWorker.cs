using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

internal interface IConsumerWorker : IWorker
{
    ValueTask Enqueue(ConsumeResult<byte[], byte[]> message, CancellationToken stopCancellationToken = default);

    Task Start();

    Task Stop();
}