using System;
using Confluent.Kafka;

namespace Erm.KafkaClient.Configuration;

internal class ProducerConfiguration : IProducerConfiguration
{
    private readonly ProducerConfig _kafkaProducerConfig;

    public ProducerConfiguration(
        ClusterConfiguration cluster,
        ProducerConfig kafkaProducerConfig,
        string name,
        string defaultTopic,
        Action<string> statisticsHandler,
        ProducerFactory factory)
    {
        _kafkaProducerConfig = kafkaProducerConfig;
        Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
        Name = name;
        DefaultTopic = defaultTopic;
        StatisticsHandler = statisticsHandler;
        Factory = factory;
    }

    public ClusterConfiguration Cluster { get; }

    public string Name { get; }

    public string DefaultTopic { get; }
        
    public Action<string> StatisticsHandler { get; }

    public ProducerFactory Factory { get; }

    public ProducerConfig GetKafkaConfig()
    {
        _kafkaProducerConfig.BootstrapServers = string.Join(",", Cluster.Brokers);
        return _kafkaProducerConfig;
    }
 
}