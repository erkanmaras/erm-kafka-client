using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Consumers.DistributionStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace Erm.KafkaClient.Configuration;

internal sealed class ConsumerConfigurationBuilder : IConsumerConfigurationBuilder
{
    private readonly List<string> _topics = new();
    private TimeSpan _autoCommitInterval = TimeSpan.FromSeconds(5);
    private AutoOffsetReset? _autoOffsetReset;
    private bool _autoStoreOffsets = true;
    private int _bufferSize;
    private ConsumerConfig _consumerConfig;
    private Type _consumerHandlerType;
    private Factory<IDistributionStrategy> _distributionStrategyFactory = _ => new BytesSumDistributionStrategy();
    private ConsumerFactory _factory = (consumer, _) => consumer;
    private string _groupId;
    private int? _heartbeatIntervalMs;
    private int? _maxPollIntervalMs;
    private string _name;
    private Action<IServiceProvider, List<TopicPartition>> _partitionAssignedHandler;
    private Action<IServiceProvider, List<TopicPartitionOffset>> _partitionRevokedHandler;
    private int? _sessionTimeoutMs;
    private Action<string> _statisticsHandler;
    private int _statisticsInterval;
    private int _workersCount;

    public ConsumerConfigurationBuilder(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }

    public IConsumerConfigurationBuilder Topic(string topicName)
    {
        _topics.Add(topicName);
        return this;
    }

    public IConsumerConfigurationBuilder Topics(IEnumerable<string> topicNames)
    {
        _topics.AddRange(topicNames);
        return this;
    }

    public IConsumerConfigurationBuilder Topics(params string[] topicNames)
    {
        return Topics(topicNames.AsEnumerable());
    }

    public IConsumerConfigurationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IConsumerConfigurationBuilder WithGroupId(string groupId)
    {
        _groupId = groupId;
        return this;
    }

    public IConsumerConfigurationBuilder WithAutoOffsetReset(AutoOffsetReset autoOffsetReset)
    {
        _autoOffsetReset = autoOffsetReset;
        return this;
    }
 
    public IConsumerConfigurationBuilder WithAutoCommitIntervalMs(int autoCommitIntervalMs)
    {
        _autoCommitInterval = TimeSpan.FromMilliseconds(autoCommitIntervalMs);
        return this;
    }
        
    public IConsumerConfigurationBuilder WithWorkersCount(int workersCount)
    {
        _workersCount = Math.Max(workersCount, 1);
        return this;
    }

    public IConsumerConfigurationBuilder WithBufferSize(int size)
    {
        _bufferSize = size;
        return this;
    }

        
    public IConsumerConfigurationBuilder WithSessionTimeoutMs(int? sessionTimeoutMs)
    {
        _sessionTimeoutMs = sessionTimeoutMs;
        return this;
    }

    public IConsumerConfigurationBuilder WithHeartbeatIntervalMs(int? heartbeatIntervalMs)
    {
        _heartbeatIntervalMs = heartbeatIntervalMs;
        return this;
    }

    public IConsumerConfigurationBuilder WithMaxPollIntervalMs(int? maxPollIntervalMs)
    {
        _maxPollIntervalMs = maxPollIntervalMs;
        return this;
    }
        
    public IConsumerConfigurationBuilder WithWorkDistributionStrategy<T>(Factory<T> factory) where T : class, IDistributionStrategy
    {
        _distributionStrategyFactory = factory;
        return this;
    }

    public IConsumerConfigurationBuilder WithWorkDistributionStrategy<T>() where T : class, IDistributionStrategy
    {
        ServiceCollection.AddTransient<T>();
        _distributionStrategyFactory = resolver => resolver.GetService<T>();

        return this;
    }

    public IConsumerConfigurationBuilder WithAutoStoreOffsets()
    {
        _autoStoreOffsets = true;
        return this;
    }

    public IConsumerConfigurationBuilder WithManualStoreOffsets()
    {
        _autoStoreOffsets = false;
        return this;
    }

    public IConsumerConfigurationBuilder WithHandler<THandler>() where THandler : class, IConsumerMessageHandler
    {
        _consumerHandlerType = typeof(THandler);
        ServiceCollection.AddTransient<IConsumerMessageHandler, THandler>();
        return this;
    }

    public IConsumerConfigurationBuilder WithStatisticsHandler(Action<string> statisticsHandler)
    {
        _statisticsHandler = statisticsHandler;
        return this;
    }

    public IConsumerConfigurationBuilder WithStatisticsIntervalMs(int statisticsIntervalMs)
    {
        _statisticsInterval = statisticsIntervalMs;
        return this;
    }

    public IConsumerConfigurationBuilder WithConsumerConfig(ConsumerConfig config)
    {
        _consumerConfig = config;
        return this;
    }

    public IConsumerConfigurationBuilder WithPartitionsAssignedHandler(Action<IServiceProvider, List<TopicPartition>> partitionsAssignedHandler)
    {
        _partitionAssignedHandler = partitionsAssignedHandler;
        return this;
    }

    public IConsumerConfigurationBuilder WithPartitionsRevokedHandler(Action<IServiceProvider, List<TopicPartitionOffset>> partitionsRevokedHandler)
    {
        _partitionRevokedHandler = partitionsRevokedHandler;
        return this;
    }

    public IConsumerConfigurationBuilder WithCustomFactory(ConsumerFactory factory)
    {
        _factory = factory;
        return this;
    }

    public IConsumerConfiguration Build(ClusterConfiguration clusterConfiguration)
    {
        _consumerConfig ??= new ConsumerConfig();
        _consumerConfig.BootstrapServers ??= string.Join(",", clusterConfiguration.Brokers);
        _consumerConfig.GroupId ??= _groupId;
        _consumerConfig.AutoOffsetReset ??= _autoOffsetReset;
        _consumerConfig.SessionTimeoutMs ??= _sessionTimeoutMs;
        _consumerConfig.HeartbeatIntervalMs ??= _heartbeatIntervalMs;
        _consumerConfig.MaxPollIntervalMs ??= _maxPollIntervalMs;
        _consumerConfig.StatisticsIntervalMs ??= _statisticsInterval;
        _consumerConfig.EnableAutoOffsetStore = false;
        _consumerConfig.EnableAutoCommit = false;
        _consumerConfig.ReadSecurityInformation(clusterConfiguration);

        return new ConsumerConfiguration(
            _consumerConfig,
            _name,
            _topics,
            _workersCount,
            _bufferSize,
            _autoStoreOffsets,
            _autoCommitInterval,
            _consumerHandlerType,
            _statisticsHandler,
            _distributionStrategyFactory,
            _partitionAssignedHandler,
            _partitionRevokedHandler,
            _factory);
    }
}