using System;
using System.Threading;
using Confluent.Kafka;
using FluentAssertions;
using Erm.KafkaClient.Consumers;
using Xunit;

namespace Erm.KafkaClient.Tests;

public class ConsumerContextTests
{
    [Fact]
    public void MessageTimestamp_ConsumeResultHasMessageTimestamp_ReturnsMessageTimestampFromResult()
    {
        // Arrange
        var expectedMessageTimestamp = new DateTime(
            2020,
            1,
            1,
            0,
            0,
            0);

        var consumerResult = new ConsumeResult<byte[], byte[]>
        {
            Message = new Message<byte[], byte[]>
            {
                Timestamp = new Timestamp(expectedMessageTimestamp)
            }
        };

        var target = new ConsumerContext(
            null,
            null,
            consumerResult,
            0,CancellationToken.None);

        // Act
        var messageTimestamp = target.MessageTimestamp;

        // Assert
        messageTimestamp.Should().Be(expectedMessageTimestamp.ToUniversalTime());
    }
}