using System;
using Erm.KafkaClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class ConsumerManagerFactory : IConsumerManagerFactory
{
    public IConsumerManager Create(IConsumerConfiguration configuration, IServiceProvider serviceProvider)
    {
        var consumer = configuration.Factory(new Consumer(configuration, serviceProvider, serviceProvider.GetService<ILogger<Consumer>>()), serviceProvider);

        var consumerWorkerPool = new ConsumerWorkerPool(
            consumer,
            serviceProvider,
            serviceProvider.GetRequiredService<IConsumerMessageHandler>(),
            configuration.DistributionStrategyFactory);

        var feeder = new WorkerPoolFeeder(
            consumer,
            consumerWorkerPool,
            serviceProvider.GetService<ILogger<WorkerPoolFeeder>>());

        var consumerManager = new ConsumerManager(
            consumer,
            consumerWorkerPool,
            feeder,
            serviceProvider.GetService<ILogger<ConsumerManager>>());

        return consumerManager;
    }
}