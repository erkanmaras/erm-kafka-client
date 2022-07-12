using System;

namespace Erm.KafkaClient.Configuration;

/// <summary>
///     A builder to configure Erm.KafkaClient
/// </summary>
public interface IKafkaClientConfigurationBuilder
{
    /// <summary>
    ///     Adds a new Cluster
    /// </summary>
    /// <param name="cluster">A handle to configure the cluster</param>
    /// <returns></returns>
    IKafkaClientConfigurationBuilder AddCluster(Action<IClusterConfigurationBuilder> cluster);
}