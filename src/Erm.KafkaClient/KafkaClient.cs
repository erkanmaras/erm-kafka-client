using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Erm.KafkaClient.Configuration;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient;

internal class KafkaClient : IKafkaClient
{
    private readonly KafkaConfiguration _configuration;
    private readonly IConsumerManagerFactory _consumerManagerFactory;
    private readonly List<IConsumerManager> _consumerManagers = new();
    private readonly IServiceProvider _serviceProvider;

    public KafkaClient(
        IServiceProvider serviceProvider,
        KafkaConfiguration configuration,
        IConsumerManagerFactory consumerManagerFactory,
        IConsumerAccessor consumers,
        IProducerAccessor producers)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _consumerManagerFactory = consumerManagerFactory;
        Consumers = consumers;
        Producers = producers;
    }

    public IConsumerAccessor Consumers { get; }
    public IProducerAccessor Producers { get; }

    public async Task Start(CancellationToken stopCancellationToken = default)
    {
        var stopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stopCancellationToken);
        stopTokenSource.Token.Register(() => Stop().GetAwaiter().GetResult());

        foreach (var cluster in _configuration.Clusters)
        {
            foreach (var consumerConfiguration in cluster.Consumers)
            {
                var serviceScope = _serviceProvider.CreateScope();
                var consumerManager = _consumerManagerFactory.Create(consumerConfiguration, serviceScope.ServiceProvider);
                _consumerManagers.Add(consumerManager);
                Consumers.Add(new KafkaConsumer(consumerManager, serviceScope.ServiceProvider.GetService<ILogger<KafkaConsumer>>()));
                await consumerManager.Start().ConfigureAwait(false);
            }
        }
    }

    public Task Stop()
    {
        return Task.WhenAll(_consumerManagers.Select(x => x.Stop()));
    }
}