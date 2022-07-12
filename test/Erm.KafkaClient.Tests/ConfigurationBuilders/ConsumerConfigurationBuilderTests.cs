using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Erm.KafkaClient.Configuration;
using Erm.KafkaClient.Consumers;
using Xunit;

namespace Erm.KafkaClient.Tests.ConfigurationBuilders;

public class ConsumerConfigurationBuilderTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IServiceCollection> _dependencyConfiguratorMock;

    private readonly ConsumerConfigurationBuilder _target;
        
    public ConsumerConfigurationBuilderTests()
    {
        _dependencyConfiguratorMock = new Mock<IServiceCollection>();

        _target = new ConsumerConfigurationBuilder(_dependencyConfiguratorMock.Object);
    }

    [Fact]
    public void DependencyConfigurator_SetProperty_ReturnPassedInstance()
    {
        // Assert
        _target.ServiceCollection.Should().BeEquivalentTo(_dependencyConfiguratorMock.Object);
    }

    [Fact]
    public void Build_RequiredCalls_ReturnDefaultValues()
    {
        // Arrange
        var clusterConfiguration = _fixture.Create<ClusterConfiguration>();
          
        var topic1 = _fixture.Create<string>();
        const int bufferSize = 100;
        const int workers = 10;
        var groupId = _fixture.Create<string>();

        _target
            .Topics(topic1)
            .WithBufferSize(bufferSize)
            .WithWorkersCount(workers)
            .WithGroupId(groupId);

        // Act
        var configuration = _target.Build(clusterConfiguration);

        // Assert
        configuration.Topics.Should().BeEquivalentTo(topic1);
        configuration.BufferSize.Should().Be(bufferSize);
        configuration.WorkerCount.Should().Be(workers);
        configuration.GroupId.Should().Be(groupId);
        configuration.GetKafkaConfig().AutoOffsetReset.Should().BeNull();
        configuration.GetKafkaConfig().EnableAutoOffsetStore.Should().Be(false);
        configuration.GetKafkaConfig().EnableAutoCommit.Should().Be(false);
        configuration.AutoStoreOffsets.Should().Be(true);
        configuration.AutoCommitInterval.Should().Be(TimeSpan.FromSeconds(5));
        configuration.StatisticsHandler.Should().BeNull();
        configuration.PartitionsAssignedHandler.Should().BeNull();
        configuration.PartitionsRevokedHandler.Should().BeNull();
        configuration.ConsumerHandlerType.Should().BeNull();
    }

    [Fact]
    public void Build_AllCalls_ReturnPassedValues()
    {
        // Arrange
        var clusterConfiguration = _fixture.Create<ClusterConfiguration>();
        var topic1 = _fixture.Create<string>();
        var topic2 = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        const int bufferSize = 100;
        const int workers = 10;
        const AutoOffsetReset offsetReset = AutoOffsetReset.Earliest;
        var groupId = _fixture.Create<string>();
        const int autoCommitInterval = 10000;
        const int maxPollIntervalMs = 500000;
        ConsumerFactory factory = (producer, _) => producer;
        Action<string> statisticsHandler = _ => { };
        Action<IServiceProvider, List<TopicPartition>> partitionsAssignedHandler = (_, _) => { };
        Action<IServiceProvider, List<TopicPartitionOffset>> partitionsRevokedHandler = (_, _) => { };
        const int statisticsIntervalMs = 100;
        var consumerConfig = new ConsumerConfig();

        _target
            .Topics(topic1)
            .Topic(topic2)
            .WithName(name)
            .WithBufferSize(bufferSize)
            .WithWorkersCount(workers)
            .WithGroupId(groupId)
            .WithAutoOffsetReset(offsetReset)
            .WithManualStoreOffsets()
            .WithAutoCommitIntervalMs(autoCommitInterval)
            .WithMaxPollIntervalMs(maxPollIntervalMs)
            .WithConsumerConfig(consumerConfig)
            .WithCustomFactory(factory)
            .WithStatisticsIntervalMs(statisticsIntervalMs)
            .WithStatisticsHandler(statisticsHandler)
            .WithPartitionsAssignedHandler(partitionsAssignedHandler)
            .WithPartitionsRevokedHandler(partitionsRevokedHandler)
            .WithHandler<TestMessageMessageHandler>();

        // Act
        var configuration = _target.Build(clusterConfiguration);

        // Assert
        configuration.Topics.Should().BeEquivalentTo(topic1, topic2);
        configuration.ConsumerName.Should().Be(name);
        configuration.BufferSize.Should().Be(bufferSize);
        configuration.WorkerCount.Should().Be(workers);
        configuration.GroupId.Should().Be(groupId);
        configuration.GroupId.Should().Be(groupId);
        configuration.GetKafkaConfig().AutoOffsetReset.Should().Be((AutoOffsetReset)(int)offsetReset);
        configuration.AutoStoreOffsets.Should().Be(false);
        configuration.GetKafkaConfig().EnableAutoOffsetStore.Should().Be(false);
        configuration.GetKafkaConfig().EnableAutoCommit.Should().Be(false);
        configuration.AutoCommitInterval.Should().Be(TimeSpan.FromMilliseconds(autoCommitInterval));
        configuration.GetKafkaConfig().MaxPollIntervalMs.Should().Be(maxPollIntervalMs);
        configuration.GetKafkaConfig().StatisticsIntervalMs.Should().Be(statisticsIntervalMs);
        configuration.StatisticsHandler.Should().BeEquivalentTo(statisticsHandler);
        configuration.PartitionsAssignedHandler.Should().BeEquivalentTo(partitionsAssignedHandler);
        configuration.PartitionsRevokedHandler.Should().BeEquivalentTo( partitionsRevokedHandler);
        configuration.GetKafkaConfig().Should().BeSameAs(consumerConfig);
        configuration.ConsumerHandlerType.Should().Be<TestMessageMessageHandler>();
    }
}
public class TestMessageMessageHandler : IConsumerMessageHandler
{
    public Task Handle(IKafkaMessageContext context)
    {
        return Task.CompletedTask;
    }
}