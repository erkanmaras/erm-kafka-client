using System;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

/// <summary>
///     A class to configure Erm.KafkaClient
/// </summary>
public class KafkaClientConfigurator
{
    private readonly KafkaConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KafkaClientConfigurator" /> class.
    /// </summary>
    /// <param name="serviceCollection">Dependency injection configurator</param>
    /// <param name="kafka">A handler to setup the configuration</param>
    public KafkaClientConfigurator(
        IServiceCollection serviceCollection,
        Action<IKafkaClientConfigurationBuilder> kafka)
    {
        var builder = new KafkaClientConfigurationBuilder(serviceCollection);

        kafka(builder);

        _configuration = builder.Build();
    }

    /// <summary>
    ///     Creates the Erm.KafkaClient bus
    /// </summary>
    /// <param name="resolver">The <see cref="IServiceProvider" /> to be used by the framework</param>
    /// <returns></returns>
    public IKafkaClient Create(IServiceProvider resolver)
    {
        var scope = resolver.CreateScope();

        return new KafkaClient(
            scope.ServiceProvider,
            _configuration,
            scope.ServiceProvider.GetService<IConsumerManagerFactory>(),
            scope.ServiceProvider.GetService<IConsumerAccessor>(),
            scope.ServiceProvider.GetService<IProducerAccessor>());
    }
}