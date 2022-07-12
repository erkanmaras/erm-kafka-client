using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

internal class ProducerConfigurationBuilder : IProducerConfigurationBuilder
{
    private readonly string _name;
    private Acks? _acks;
    private bool? _idempotence;
    private double? _lingerMs;
    private int? _retryBackoffMs;
    private int? _maxRetries;
    private ProducerConfig _producerConfig;
    private ProducerFactory _factory = (producer, _) => producer;
    private Action<string> _statisticsHandler;
    private int? _statisticsInterval;
    private string _topic;

    public ProducerConfigurationBuilder(IServiceCollection serviceCollection, string name)
    {
        _name = name;
        ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }

    public IProducerConfigurationBuilder DefaultTopic(string topic)
    {
        _topic = topic;
        return this;
    }

    public IProducerConfigurationBuilder WithAcks(Acks acks)
    {
        _acks = acks;
        return this;
    }
        
    public IProducerConfigurationBuilder WithLingerMs(double? lingerMs)
    {
        _lingerMs = lingerMs;
        return this;
    }
        
    public IProducerConfigurationBuilder WithIdempotence(bool? idempotence)
    {
        _idempotence = idempotence;
        return this;
    }

    public IProducerConfigurationBuilder WithMaxRetries(int? maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }
        
    public IProducerConfigurationBuilder WithRetryBackoffMs(int? retryBackoffMs)
    {
        _retryBackoffMs = retryBackoffMs;
        return this;
    }


    public IProducerConfigurationBuilder WithStatisticsHandler(Action<string> statisticsHandler)
    {
        _statisticsHandler = statisticsHandler;
        return this;
    }

    public IProducerConfigurationBuilder WithStatisticsIntervalMs(int? statisticsIntervalMs)
    {
        _statisticsInterval = statisticsIntervalMs;
        return this;
    }

    public IProducerConfigurationBuilder WithProducerConfig(ProducerConfig config)
    {
        _producerConfig = config;
        return this;
    }

    public IProducerConfigurationBuilder WithCompression(CompressionType compressionType, int? compressionLevel)
    {
        _producerConfig ??= new ProducerConfig();
        _producerConfig.CompressionType = compressionType;
        _producerConfig.CompressionLevel = compressionLevel;

        return this;
    }

    public IProducerConfigurationBuilder WithCustomFactory(ProducerFactory factory)
    {
        _factory = factory;
        return this;
    }

    public IProducerConfiguration Build(ClusterConfiguration clusterConfiguration)
    {
        _producerConfig ??= new ProducerConfig();
        _producerConfig.StatisticsIntervalMs = _statisticsInterval;
        _producerConfig.LingerMs ??= _lingerMs;
        _producerConfig.Acks ??= _acks;
        _producerConfig.EnableIdempotence ??= _idempotence;
        _producerConfig.MessageSendMaxRetries ??= _maxRetries;
        _producerConfig.RetryBackoffMs ??= _retryBackoffMs;
        _producerConfig.ReadSecurityInformation(clusterConfiguration);

        var configuration = new ProducerConfiguration(
            clusterConfiguration,
            _producerConfig,
            _name,
            _topic,
               
            _statisticsHandler,
            _factory);

        return configuration;
    }
}