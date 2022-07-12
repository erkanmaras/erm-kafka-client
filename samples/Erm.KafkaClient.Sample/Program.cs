using System;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;

namespace Erm.KafkaClient.Sample;

internal static class Program
{
    private static async Task Main()
    {
        var brokerAddress = "localhost:9077";
        var topic = "test-topic";
        KafkaClient.CreateTopicsIfNotExist(brokerAddress, new[] { topic });
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddFilter("*", LogLevel.Trace);
        });

        const string producerName = "PrintConsoleProducer";
        const string consumerName = "PrintConsoleConsumer";

        services.AddKafkaClient(
            kafka => kafka
                .AddCluster(
                    cluster => cluster
                        .WithBrokers(new[] { brokerAddress })
                        .AddProducer(
                            producerName,
                            producer => producer
                                .DefaultTopic(topic)
                                .WithAcks(Acks.All)
                        )
                        .AddConsumer(
                            consumer => consumer
                                .Topic(topic)
                                .WithGroupId(consumerName + "Group")
                                .WithName(consumerName)
                                .WithBufferSize(100)
                                .WithWorkersCount(10)
                                .WithSessionTimeoutMs(10 * 1000)
                                .WithHeartbeatIntervalMs(3 * 1000)
                                .WithMaxPollIntervalMs(20 * 1000)
                                .WithAutoOffsetReset(AutoOffsetReset.Latest)
                                .WithHandler<PrintConsoleMessageHandler>()
                        )
                )
        );


        var provider = services.BuildServiceProvider();
        var bus = provider.CreateKafkaClient();
        await bus.Start();
        var consumers = provider.GetRequiredService<IConsumerAccessor>();
        var producers = provider.GetRequiredService<IProducerAccessor>();

        Console.WriteLine("Options send, restart, exit");
        while (true)
        {
            var input = Console.ReadLine()?.ToLower();

            switch (input)
            {
                case "send":
                    IKafkaMessageHeaders header = new KafkaMessageHeaders();
                    header.AddString("NullHeader", null);
                    for (var i = 0; i < 100; i++)
                    {
                        await producers[producerName].ProduceAsync(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), Encoding.UTF8.GetBytes($"Message: {Guid.NewGuid()}"),
                            header);
                        Console.WriteLine($"Message {i} send!");
                    }

                    break;
                case "restart":
                    foreach (var consumer in consumers.All)
                    {
                        await consumer.Restart();
                    }

                    Console.WriteLine("Consumer paused");
                    break;

                case "exit":
                    await bus.Stop();
                    return;
            }
        }
    }
}