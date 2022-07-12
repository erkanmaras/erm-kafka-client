using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using Erm.KafkaClient.Consumers;
using Xunit;

namespace Erm.KafkaClient.Tests;

public class OffsetCommitterTests : IDisposable
{
    private const int TestTimeout = 1000;
    private readonly Mock<IConsumer> _consumerMock;
    private readonly OffsetCommitter _offsetCommitter;
    private readonly TopicPartition _topicPartition;
        
    public OffsetCommitterTests()
    {
        _consumerMock = new Mock<IConsumer>();
        var logHandlerMock = new Mock<ILogger<OffsetCommitter>>();
        _topicPartition = new TopicPartition("topic-A", new Partition(1));
        _consumerMock.Setup(c => c.Configuration.AutoCommitInterval).Returns(TimeSpan.FromMilliseconds(10));
        _offsetCommitter = new OffsetCommitter(_consumerMock.Object, logHandlerMock.Object);
    }

    public void Dispose()
    {
        _offsetCommitter?.Dispose();
    }


    [Fact]
    public void StoreOffset_ShouldCommit()
    {
        // Arrange
        var offset = new TopicPartitionOffset(_topicPartition, new Offset(1));
        var expectedOffsets = new[] { offset };

        var ready = new ManualResetEvent(false);

        _consumerMock
            .Setup(c => c.Commit(It.Is<IEnumerable<TopicPartitionOffset>>(l => l.SequenceEqual(expectedOffsets))))
            .Callback((IEnumerable<TopicPartitionOffset> _) => { ready.Set(); });

        // Act
        _offsetCommitter.StoreOffset(offset);
        ready.WaitOne(TestTimeout);

        // Assert
        _consumerMock.Verify(
            c => c.Commit(It.Is<IEnumerable<TopicPartitionOffset>>(l => l.SequenceEqual(expectedOffsets))),
            Times.Once);
    }

    [Fact]
    public void StoreOffset_WithFailure_ShouldRequeueFailedOffsetAndCommit()
    {
        // Arrange
        var offset = new TopicPartitionOffset(_topicPartition, new Offset(2));
        var expectedOffsets = new[] { offset };

        var ready = new ManualResetEvent(false);
        var hasThrown = false;

        _consumerMock
            .Setup(c => c.Commit(It.Is<IEnumerable<TopicPartitionOffset>>(l => l.SequenceEqual(expectedOffsets))))
            .Callback((IEnumerable<TopicPartitionOffset> _) =>
            {
                if (!hasThrown)
                {
                    hasThrown = true;
                    throw new InvalidOperationException();
                }

                ready.Set();
            });

        // Act
        _offsetCommitter.StoreOffset(offset);
        ready.WaitOne(TestTimeout);

        // Assert
        _consumerMock.Verify(
            c => c.Commit(It.Is<IEnumerable<TopicPartitionOffset>>(l => l.SequenceEqual(expectedOffsets))),
            Times.Exactly(2));
    }
}