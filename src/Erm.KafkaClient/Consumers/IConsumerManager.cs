using System.Threading.Tasks;

namespace Erm.KafkaClient.Consumers;

internal interface IConsumerManager
{
    IWorkerPoolFeeder Feeder { get; }

    IConsumerWorkerPool WorkerPool { get; }

    IConsumer Consumer { get; }

    Task Start();

    Task Stop();
}