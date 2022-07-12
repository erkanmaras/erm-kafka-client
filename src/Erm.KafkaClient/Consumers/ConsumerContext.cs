using System;
using System.Threading;
using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

internal class ConsumerContext : IConsumerContext
{
    private readonly IConsumer _consumer;
    private readonly ConsumeResult<byte[], byte[]> _kafkaResult;
    private readonly IOffsetManager _offsetManager;

    public ConsumerContext(
        IConsumer consumer,
        IOffsetManager offsetManager,
        ConsumeResult<byte[], byte[]> kafkaResult,
        int workerId,
        CancellationToken workerStopped)
    {
        WorkerStopped = workerStopped;
        WorkerId = workerId;
        _consumer = consumer;
        _offsetManager = offsetManager;
        _kafkaResult = kafkaResult;
    }

    public string ConsumerName => _consumer.Configuration.ConsumerName;

    public CancellationToken WorkerStopped { get; }

    public int WorkerId { get; }

    public string Topic => _kafkaResult.Topic;

    public int Partition => _kafkaResult.Partition.Value;

    public long Offset => _kafkaResult.Offset.Value;

    public string GroupId => _consumer.Configuration.GroupId;

    public bool ShouldStoreOffset { get; set; } = true;

    public DateTime MessageTimestamp => _kafkaResult.Message.Timestamp.UtcDateTime;

    public void StoreOffset()
    {
        _offsetManager.MarkAsProcessed(_kafkaResult.TopicPartitionOffset);
    }

}