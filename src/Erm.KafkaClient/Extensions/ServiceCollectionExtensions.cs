using System;
using Erm.KafkaClient.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient;

/// <summary>
///     Extension methods over IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Configures Erm.KafkaClient
    /// </summary>
    /// <param name="services">Instance of <see cref="IServiceCollection" /></param>
    /// <param name="kafkaClient">A handler to configure Erm.KafkaClient</param>
    /// <returns></returns>
    public static IServiceCollection AddKafkaClient(this IServiceCollection services, Action<IKafkaClientConfigurationBuilder> kafkaClient)
    {
        var configurator = new KafkaClientConfigurator(services, kafkaClient);
        services.AddHostedService<KafkaClientHostedService>();
        return services.AddSingleton(configurator);
    }
}