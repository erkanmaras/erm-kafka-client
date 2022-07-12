using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;

namespace Erm.KafkaClient.IntegrationTests;

public class KafkaClientScope : IDisposable
{
    private readonly bool _consume;
    private IKafkaClient _host;

    public string ProducerName { get; }
    public string ConsumerName { get; }
    public string TopicName { get; }
    public IKafkaProducer Producer = null!;

    public KafkaTestFixtureStore ConsumedMessageStore = null!;

    public KafkaClientScope(string? namePrefix = null, bool consume = true)
    {
        namePrefix ??= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        _consume = consume;
        ProducerName = namePrefix + "_Producer";
        ConsumerName = namePrefix + "_Consumer";
        TopicName = namePrefix + "_Topic";
        _host = Start();
        Thread.Sleep(2 * 1000);
    }

    private IKafkaClient Start()
    {
        var configurationBuilder = new ConfigurationManager()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appsettings.json", optional: true, reloadOnChange: false);

        var configuration = configurationBuilder.Build();
        var brokerAddress = configuration["Kafka:BrokerAddress"];
        KafkaAdminClient.CreateTopicsIfNotExist(brokerAddress, new[] { TopicName });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddFilter("*", LogLevel.Trace);
        });

        serviceCollection.AddSingleton<KafkaTestFixtureStore>();
        serviceCollection.AddKafkaClient(
            kafka => kafka
                .AddCluster(
                    cluster =>
                    {
                        cluster.WithBrokers(new[] { brokerAddress });
                        cluster.AddProducer(
                            ProducerName,
                            producer => producer
                                .DefaultTopic(TopicName)
                                .WithAcks(Acks.Leader)
                        );
                        if (_consume)
                        {
                            cluster.AddConsumer(
                                consumer => consumer
                                    .Topic(TopicName)
                                    .WithGroupId(ConsumerName + "Group")
                                    .WithName(ConsumerName)
                                    .WithBufferSize(100)
                                    .WithWorkersCount(10)
                                    .WithSessionTimeoutMs(10 * 1000)
                                    .WithHeartbeatIntervalMs(3 * 1000)
                                    .WithMaxPollIntervalMs(20 * 1000)
                                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                                    .WithHandler<KafkaTestFixtureHandler>()
                            );
                        }
                    })
        );


        var provider = serviceCollection.BuildServiceProvider();
        var bus = provider.CreateKafkaClient();

        Producer = provider.GetRequiredService<IProducerAccessor>()[ProducerName];
        ConsumedMessageStore = provider.GetRequiredService<KafkaTestFixtureStore>();
        bus.Start();
        return bus;
    }

    public async Task WaitForConsume(int expectedCount, int timeoutInSecond = 15)
    {
        for (var i = 0; i < timeoutInSecond; i++)
        {
            await Task.Delay(1000);
            if (ConsumedMessageStore.Messages.Count == expectedCount)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _host.Stop().GetAwaiter().GetResult();
    }
}

public class KafkaTestFixtureHandler : IConsumerMessageHandler
{
    private readonly KafkaTestFixtureStore _store;

    public KafkaTestFixtureHandler(KafkaTestFixtureStore store)
    {
        _store = store;
    }

    public Task Handle(IKafkaMessageContext context)
    {
        var message = Encoding.UTF8.GetString(context.KafkaMessage.Value);
        Console.WriteLine("Message Arrived Partition: {0} | Offset: {1} | Message: {2}", context.ConsumerContext.Partition, context.ConsumerContext.Offset, message);
        _store.Messages.Add(message);
        return Task.CompletedTask;
    }
}

public class KafkaTestFixtureStore
{
    public ConcurrentBag<string> Messages = new();
}