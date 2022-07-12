using System;
using System.Collections.Generic;
using System.Linq;
using Erm.KafkaClient.Producers;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

internal class ClusterConfigurationBuilder : IClusterConfigurationBuilder
{
    private readonly List<ConsumerConfigurationBuilder> _consumers = new();
    private readonly List<ProducerConfigurationBuilder> _producers = new();
    private IEnumerable<string> _brokers;
    private Func<SecurityInformation> _securityInformationHandler;

    public ClusterConfigurationBuilder(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }

    public IClusterConfigurationBuilder WithBrokers(IEnumerable<string> brokers)
    {
        _brokers = brokers;
        return this;
    }

    public IClusterConfigurationBuilder WithSecurityInformation(Action<SecurityInformation> handler)
    {
        // Uses a handler to avoid in-memory stored passwords for long periods
        _securityInformationHandler = () =>
        {
            var config = new SecurityInformation();
            handler(config);
            return config;
        };

        return this;
    }

    public IClusterConfigurationBuilder AddProducer<TProducer>(Action<IProducerConfigurationBuilder> producer)
    {
        ServiceCollection.AddSingleton<IKafkaProducer<TProducer>>(resolver => new TypedKafkaProducer<TProducer>(resolver.GetService<IProducerAccessor>()?.GetProducer<TProducer>()));
        return AddProducer(typeof(TProducer).FullName, producer);
    }

    public IClusterConfigurationBuilder AddProducer(string name, Action<IProducerConfigurationBuilder> producer)
    {
        var builder = new ProducerConfigurationBuilder(ServiceCollection, name);
        producer(builder);
        _producers.Add(builder);
        return this;
    }

    public IClusterConfigurationBuilder AddConsumer(Action<IConsumerConfigurationBuilder> consumer)
    {
        var builder = new ConsumerConfigurationBuilder(ServiceCollection);
        consumer(builder);
        _consumers.Add(builder);
        return this;
    }

    public ClusterConfiguration Build(KafkaConfiguration kafkaConfiguration)
    {
        var configuration = new ClusterConfiguration(
            kafkaConfiguration,
            _brokers.ToList(),
            _securityInformationHandler);

        configuration.AddProducers(_producers.Select(x => x.Build(configuration)));
        configuration.AddConsumers(_consumers.Select(x => x.Build(configuration)));

        return configuration;
    }
}