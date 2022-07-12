using System;
using System.Text;
using System.Threading.Tasks;
using Erm.KafkaClient.Consumers;

namespace Erm.KafkaClient.Sample;

public class PrintConsoleMessageHandler : IConsumerMessageHandler
{
    public Task Handle(IKafkaMessageContext context)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Message Arrived Partition: {0} | Offset: {1} | Message: {2}", context.ConsumerContext.Partition, context.ConsumerContext.Offset, Encoding.UTF8.GetString(context.KafkaMessage.Value));
        Console.ForegroundColor = ConsoleColor.Blue;
        return Task.CompletedTask;
    }
}