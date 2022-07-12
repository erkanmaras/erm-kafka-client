using System.Threading.Tasks;

namespace Erm.KafkaClient.Consumers;

internal interface IWorkerPoolFeeder
{
    void Start();

    Task Stop();
}