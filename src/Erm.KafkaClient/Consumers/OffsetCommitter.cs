using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class OffsetCommitter : IOffsetCommitter
{
    private readonly Timer _commitTimer;
    private readonly IConsumer _consumer;
    private readonly ILogger<OffsetCommitter> _logger;
    private ConcurrentDictionary<(string, int), TopicPartitionOffset> _offsetsToCommit = new();

    public OffsetCommitter(IConsumer consumer, ILogger<OffsetCommitter> logger)
    {
        _consumer = consumer;
        _logger = logger;

        _commitTimer = new Timer(CommitHandlerTimerCallback, null,
            consumer.Configuration.AutoCommitInterval,
            consumer.Configuration.AutoCommitInterval);
    }

    public void Dispose()
    {
        _commitTimer.Dispose();
        CommitHandlerTimerCallback();
    }

    public void StoreOffset(TopicPartitionOffset tpo)
    {
        _offsetsToCommit.AddOrUpdate((tpo.Topic, tpo.Partition.Value), tpo, (_, _) => tpo);
    }

    private void CommitHandlerTimerCallback(object state = null)
    {
        var offsets = Interlocked.Exchange(ref _offsetsToCommit, new ConcurrentDictionary<(string, int), TopicPartitionOffset>());
            
        if (offsets.IsEmpty)
        {
            return;
        }
        try
        {
            _consumer.Commit(offsets.Values);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Commiting Offsets {ConsumerName}",_consumer.Configuration.ConsumerName);
            RequeueFailedOffsets(offsets.Values);
        }
    }

    private void RequeueFailedOffsets(IEnumerable<TopicPartitionOffset> offsets)
    {
        foreach (var tpo in offsets)
        {
            _offsetsToCommit.TryAdd((tpo.Topic, tpo.Partition.Value), tpo);
        }
    }
}