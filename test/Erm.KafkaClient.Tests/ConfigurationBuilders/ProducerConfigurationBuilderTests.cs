using System;
using AutoFixture;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Erm.KafkaClient.Configuration;
using Xunit;

namespace Erm.KafkaClient.Tests.ConfigurationBuilders
{
    public class ProducerConfigurationBuilderTests
    {
        private readonly Mock<IServiceCollection> _dependencyConfiguratorMock;
        private readonly Fixture _fixture = new();
        private readonly string _name;
        private readonly ProducerConfigurationBuilder _target;

        public ProducerConfigurationBuilderTests()
        {
            _dependencyConfiguratorMock = new Mock<IServiceCollection>();
            _name = _fixture.Create<string>();

            _target = new ProducerConfigurationBuilder(
                _dependencyConfiguratorMock.Object,
                _name);
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

            // Act
            var configuration = _target.Build(clusterConfiguration);

            // Assert
            configuration.Cluster.Should().Be(clusterConfiguration);
            configuration.Name.Should().Be(_name);
            configuration.DefaultTopic.Should().BeNull();
            configuration.StatisticsHandler.Should().BeNull();
        }

        [Fact]
        public void Build_AllCalls_ReturnPassedValues()
        {
            // Arrange
            var clusterConfiguration = _fixture.Create<ClusterConfiguration>();

            var defaultTopic = _fixture.Create<string>();
            var acks = _fixture.Create<Acks>();
            const int lingerMs = 50;
            const int maxRetries = 50;
            const int retryBackoffMs = 50;
            const bool idempotence = true;
            ProducerFactory factory = (producer, _) => producer;
            Action<string> statisticsHandler = _ => { };
            const int statisticsIntervalMs = 100;
            var producerConfig = new ProducerConfig();
            var compressionType = CompressionType.Lz4;
            var compressionLevel = 5;

            _target
                .DefaultTopic(defaultTopic)
                .WithAcks(acks)
                .WithMaxRetries(maxRetries)
                .WithRetryBackoffMs(retryBackoffMs)
                .WithIdempotence(idempotence)
                .WithLingerMs(lingerMs)
                .WithCustomFactory(factory)
                .WithStatisticsHandler(statisticsHandler)
                .WithStatisticsIntervalMs(statisticsIntervalMs)
                .WithProducerConfig(producerConfig)
                .WithCompression(compressionType, compressionLevel);

            // Act
            var configuration = _target.Build(clusterConfiguration);

            // Assert
            configuration.Cluster.Should().Be(clusterConfiguration);
            configuration.Name.Should().Be(_name);
            configuration.DefaultTopic.Should().Be(defaultTopic);
            configuration.StatisticsHandler.Should().NotBeNull();
            
            var kafkaConfig = configuration.GetKafkaConfig();
            kafkaConfig.Acks.Should().Be(acks);
            kafkaConfig.LingerMs.Should().Be(lingerMs);
            kafkaConfig.MessageSendMaxRetries.Should().Be(maxRetries);
            kafkaConfig.RetryBackoffMs.Should().Be(retryBackoffMs);
            kafkaConfig.EnableIdempotence.Should().Be(idempotence);
            kafkaConfig.CompressionType.Should().Be(compressionType);
            kafkaConfig.CompressionLevel.Should().Be(compressionLevel);
            kafkaConfig.StatisticsIntervalMs.Should().Be(statisticsIntervalMs);
            kafkaConfig.Should().BeSameAs(producerConfig);
        }
    }
}