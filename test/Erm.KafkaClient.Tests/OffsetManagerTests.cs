using System.Collections.Generic;
using Confluent.Kafka;
using Moq;
using Erm.KafkaClient.Consumers;
using Xunit;

namespace Erm.KafkaClient.Tests;

public class OffsetManagerTests
{
    private readonly Mock<IOffsetCommitter> _committerMock;
    private readonly OffsetManager _target;
    private readonly TopicPartition _topicPartition;


    public OffsetManagerTests()
    {
        _committerMock = new Mock<IOffsetCommitter>();
        _topicPartition = new TopicPartition("topic-A", new Partition(1));

        _target = new OffsetManager(
            _committerMock.Object,
            new List<TopicPartition> { _topicPartition });
    }

    [Fact]
    public void StoreOffset_WithInvalidTopicPartition_ShouldDoNothing()
    {
        // Arrange
        _target.AddOffset(new TopicPartitionOffset(_topicPartition, new Offset(1)));

        // Act
        _target.MarkAsProcessed(new TopicPartitionOffset(new TopicPartition("topic-B", new Partition(1)), new Offset(1)));

        // Assert
        _committerMock.Verify(c => c.StoreOffset(It.IsAny<TopicPartitionOffset>()), Times.Never());
    }

    [Fact]
    public void StoreOffset_WithGaps_ShouldStoreOffsetJustOnce()
    {
        // Arrange
        _target.AddOffset(new TopicPartitionOffset(_topicPartition, new Offset(1)));
        _target.AddOffset(new TopicPartitionOffset(_topicPartition, new Offset(2)));
        _target.AddOffset(new TopicPartitionOffset(_topicPartition, new Offset(3)));

        // Act
        _target.MarkAsProcessed(new TopicPartitionOffset(_topicPartition, new Offset(3)));
        _target.MarkAsProcessed(new TopicPartitionOffset(_topicPartition, new Offset(2)));
        _target.MarkAsProcessed(new TopicPartitionOffset(_topicPartition, new Offset(1)));

        // Assert
        _committerMock.Verify(
            c =>
                c.StoreOffset(
                    It.Is<TopicPartitionOffset>(
                        p =>
                            p.Partition.Equals(_topicPartition.Partition) &&
                            p.Offset.Value.Equals(4))),
            Times.Once);
    }
}