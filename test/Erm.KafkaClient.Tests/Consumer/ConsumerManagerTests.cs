using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Erm.KafkaClient.Configuration;
using Erm.KafkaClient.Consumers;
using Xunit;

namespace Erm.KafkaClient.Tests.Consumer;

public class ConsumerManagerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();

    private readonly Mock<IConsumer> _consumerMock;
    private readonly Mock<IWorkerPoolFeeder> _feederMock;
    private readonly Mock<ILogger<ConsumerManager>> _loggerMock;

    private Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartition>> _onPartitionAssignedHandler;
    private Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>> _onPartitionRevokedHandler;

    private readonly ConsumerManager _target;
    private readonly Mock<IConsumerWorkerPool> _workerPoolMock;


    public ConsumerManagerTests()
    {
        _consumerMock = new Mock<IConsumer>(MockBehavior.Strict);
        _workerPoolMock = new Mock<IConsumerWorkerPool>(MockBehavior.Strict);
        _feederMock = new Mock<IWorkerPoolFeeder>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<ConsumerManager>>();

        _consumerMock
            .Setup(x => x.OnPartitionsAssigned(It.IsAny<Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartition>>>()))
            .Callback((Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartition>> value) => _onPartitionAssignedHandler = value);

        _consumerMock
            .Setup(x => x.OnPartitionsRevoked(It.IsAny<Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>>>()))
            .Callback((Action<IServiceProvider, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>> value) => _onPartitionRevokedHandler = value);

        _target = new ConsumerManager(
            _consumerMock.Object,
            _workerPoolMock.Object,
            _feederMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void ConstructorCalled_InitializeProperties()
    {
        // Assert
        _target.Consumer.Should().Be(_consumerMock.Object);
        _target.WorkerPool.Should().Be(_workerPoolMock.Object);
        _target.Feeder.Should().Be(_feederMock.Object);

        _consumerMock.VerifyAll();
    }

    [Fact]
    public async Task StartAsync_StartDependencies()
    {
        // Arrange
        _feederMock
            .Setup(x => x.Start());

        // Act
        await _target.Start();

        // Assert
        _feederMock.VerifyAll();
    }

    [Fact]
    public async Task StopAsync_StopDependencies()
    {
        // Arrange
        _feederMock
            .Setup(x => x.Stop())
            .Returns(Task.CompletedTask);

        _workerPoolMock
            .Setup(x => x.Stop())
            .Returns(Task.CompletedTask);

        _consumerMock
            .Setup(x => x.Dispose());

        // Act
        await _target.Stop();

        // Assert
        _feederMock.VerifyAll();
        _workerPoolMock.VerifyAll();
        _consumerMock.VerifyAll();
    }

    [Fact]
    public void OnPartitionsAssigned_StartWorkerPool()
    {
     
        var partitions = _fixture.Create<List<TopicPartition>>();

        _workerPoolMock
            .Setup(x => x.Start(partitions))
            .Returns(Task.CompletedTask);

        _consumerMock
            .SetupGet(x => x.Configuration)
            .Returns(new Mock<IConsumerConfiguration>().Object);


        // Act
        _onPartitionAssignedHandler(_serviceProvider.Object, null, partitions);

        // Assert
        _workerPoolMock.VerifyAll();
    }

    [Fact]
    public void OnPartitionsRevoked_StopWorkerPool()
    {
 
        var partitions = _fixture.Create<List<TopicPartitionOffset>>();

        _workerPoolMock
            .Setup(x => x.Stop())
            .Returns(Task.CompletedTask);

        _consumerMock
            .SetupGet(x => x.Configuration)
            .Returns(new Mock<IConsumerConfiguration>().Object);


        // Act
        _onPartitionRevokedHandler(_serviceProvider.Object, null, partitions);

        // Assert
        _workerPoolMock.VerifyAll();
        _loggerMock.VerifyAll();
    }
}