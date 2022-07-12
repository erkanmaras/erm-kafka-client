using System;
using Erm.KafkaClient.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient;

/// <summary>
///     Extension methods over IServiceProvider
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    ///     Creates a Erm.KafkaClient bus
    /// </summary>
    /// <param name="provider">Instance of <see cref="IServiceProvider" /></param>
    /// <returns><see cref="IKafkaClient" />A Erm.KafkaClient bus</returns>
    public static IKafkaClient CreateKafkaClient(this IServiceProvider provider)
    {
        return provider.GetRequiredService<KafkaClientConfigurator>().Create(provider);
    }
}