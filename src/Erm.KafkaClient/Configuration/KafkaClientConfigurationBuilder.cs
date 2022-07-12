using System;
using System.Collections.Generic;
using System.Linq;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

internal class KafkaClientConfigurationBuilder : IKafkaClientConfigurationBuilder
{
    private readonly List<ClusterConfigurationBuilder> _clusters = new();
    private readonly IServiceCollection _serviceCollection;

    public KafkaClientConfigurationBuilder(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public IKafkaClientConfigurationBuilder AddCluster(Action<IClusterConfigurationBuilder> cluster)
    {
        var builder = new ClusterConfigurationBuilder(_serviceCollection);
        cluster(builder);
        _clusters.Add(builder);
        return this;
    }

    public KafkaConfiguration Build()
    {
        var configuration = new KafkaConfiguration();

        configuration.AddClusters(_clusters.Select(x => x.Build(configuration)));

        _serviceCollection.AddSingleton<IProducerAccessor>(
            resolver => new ProducerAccessor(
                configuration.Clusters
                    .SelectMany(x => x.Producers)
                    .Select(
                        producer => new KafkaProducer(
                            resolver,
                            producer))));

        _serviceCollection
            .AddSingleton<IConsumerAccessor>(new ConsumerAccessor())
            .AddSingleton<IConsumerManagerFactory>(new ConsumerManagerFactory());

        return configuration;
    }
}