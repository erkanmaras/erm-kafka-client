using System;
using Confluent.Kafka;

namespace Erm.KafkaClient.Configuration;

internal interface IProducerConfiguration
{
    ClusterConfiguration Cluster { get; }

    string Name { get; }

    string DefaultTopic { get; }
        
    Action<string> StatisticsHandler { get; }

    ProducerFactory Factory { get; }

    ProducerConfig GetKafkaConfig();
}