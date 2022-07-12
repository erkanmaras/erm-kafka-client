using System;
using System.Collections.Generic;
using System.Linq;

namespace Erm.KafkaClient.Configuration;

internal class ClusterConfiguration
{
    private readonly List<IConsumerConfiguration> _consumers = new();
    private readonly List<IProducerConfiguration> _producers = new();
    private readonly Func<SecurityInformation> _securityInformationHandler;

    public ClusterConfiguration(
        KafkaConfiguration kafka,
        IEnumerable<string> brokers,
        Func<SecurityInformation> securityInformationHandler)
    {
        _securityInformationHandler = securityInformationHandler;
        Kafka = kafka;
        Brokers = brokers.ToList();
    }

    public KafkaConfiguration Kafka { get; }

    public IReadOnlyCollection<string> Brokers { get; }

    public IReadOnlyCollection<IProducerConfiguration> Producers => _producers.AsReadOnly();

    public IReadOnlyCollection<IConsumerConfiguration> Consumers => _consumers.AsReadOnly();

    public void AddConsumers(IEnumerable<IConsumerConfiguration> configurations)
    {
        _consumers.AddRange(configurations);
    }

    public void AddProducers(IEnumerable<IProducerConfiguration> configurations)
    {
        _producers.AddRange(configurations);
    }

    public SecurityInformation GetSecurityInformation()
    {
        return _securityInformationHandler?.Invoke();
    }
}