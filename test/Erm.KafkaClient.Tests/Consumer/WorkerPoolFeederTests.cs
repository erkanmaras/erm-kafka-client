using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using Erm.KafkaClient.Consumers;
using Xunit;

namespace Erm.KafkaClient.Tests.Consumer;

public class WorkerPoolFeederTests
{
    private readonly Mock<IConsumer> _consumerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly WorkerPoolFeeder _target;
    private readonly Mock<IConsumerWorkerPool> _workerPoolMock;
        
    public WorkerPoolFeederTests()
    {
        _consumerMock = new Mock<IConsumer>(MockBehavior.Strict);
        _workerPoolMock = new Mock<IConsumerWorkerPool>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger>();

        _target = new WorkerPoolFeeder(
            _consumerMock.Object,
            _workerPoolMock.Object,
            _loggerMock.Object);
    }

    [Fact(Timeout = 1000)]
    public async Task StopAsync_WithoutStarting_Return()
    {
        // Act
        await _target.Stop();
    }

    [Fact(Timeout = 1000)]
    public async Task StopAsync_WaitingOnConsumeWithCancellation_MustStop()
    {
        // Arrange
        var ready = new ManualResetEvent(false);

        _consumerMock
            .Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .Returns(
                async (CancellationToken ct) =>
                {
                    ready.Set();
                    await Task.Delay(Timeout.Infinite, ct);
                    return default; // Never reached
                });

        // Act
        _target.Start();
        ready.WaitOne();
        await _target.Stop();

        // Assert
        _consumerMock.Verify(x => x.ConsumeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Timeout = 1000)]
    public async Task StopAsync_WaitingOnQueuingWithCancellation_MustStop()
    {
        // Arrange
        var consumeResult = new ConsumeResult<byte[], byte[]>();
        var ready = new ManualResetEvent(false);

        _consumerMock
            .Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(consumeResult);

        _workerPoolMock
            .Setup(x => x.Enqueue(consumeResult, It.IsAny<CancellationToken>()))
            .Returns((ConsumeResult<byte[], byte[]> _, CancellationToken ct) =>
            {
                ready.Set();
                return Task.Delay(Timeout.Infinite, ct);
            });

        // Act
        _target.Start();
        ready.WaitOne();
        await _target.Stop();

        // Assert
        _consumerMock.VerifyAll();
        _workerPoolMock.VerifyAll();
    }

    [Fact(Timeout = 1000)]
    public async Task ConsumeAsyncThrows_LogAndCallConsumeAsyncAgain()
    {
        // Arrange
        var consumeResult = new ConsumeResult<byte[], byte[]>();
        var exception = new Exception();
        var ready = new ManualResetEvent(false);

        _consumerMock
            .SetupSequence(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .Throws(exception)
            .ReturnsAsync(consumeResult);

        _workerPoolMock
            .Setup(x => x.Enqueue(consumeResult, It.IsAny<CancellationToken>()))
            .Returns((ConsumeResult<byte[], byte[]> _, CancellationToken ct) =>
            {
                ready.Set();
                return Task.Delay(Timeout.Infinite, ct);
            });

        // Act
        _target.Start();
        ready.WaitOne();
        await _target.Stop();

        // Assert
        _consumerMock.VerifyAll();
        _workerPoolMock.VerifyAll();
    }

    [Fact(Timeout = 1000)]
    public async Task EnqueueAsyncThrows_LogAndCallConsumeAsyncAgain()
    {
        // Arrange
        var consumeResult = new ConsumeResult<byte[], byte[]>();
        var exception = new Exception();
        var ready = new ManualResetEvent(false);

        _consumerMock
            .Setup(x => x.ConsumeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(consumeResult);

        var hasThrown = false;
        _workerPoolMock
            .Setup(x => x.Enqueue(consumeResult, It.IsAny<CancellationToken>()))
            .Returns((ConsumeResult<byte[], byte[]> _, CancellationToken ct) =>
            {
                ready.Set();

                if (!hasThrown)
                {
                    hasThrown = true;
                    throw exception;
                }

                return Task.Delay(Timeout.Infinite, ct);
            });


        // Act
        _target.Start();
        ready.WaitOne();
        await _target.Stop();

        // Assert
        _consumerMock.VerifyAll();
        _workerPoolMock.VerifyAll();
        _loggerMock.VerifyAll();
    }
}