using System.Threading.Tasks;

namespace Erm.KafkaClient.Consumers;

public interface IConsumerMessageHandler
{
    Task Handle(IKafkaMessageContext context);
}