using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;

namespace Erm.KafkaClient.Consumers;

internal class OffsetManager : IOffsetManager, IDisposable
{
    private readonly IOffsetCommitter _committer;
    private readonly Dictionary<(string, int), PartitionOffsets> _partitionsOffsets;

    public OffsetManager(IOffsetCommitter committer, IEnumerable<TopicPartition> partitions)
    {
        _committer = committer;
        _partitionsOffsets = partitions.ToDictionary(partition => (partition.Topic, partition.Partition.Value), _ => new PartitionOffsets());
    }

    public void Dispose()
    {
        _committer.Dispose();
    }

    public void MarkAsProcessed(TopicPartitionOffset offset)
    {
        if (!_partitionsOffsets.TryGetValue((offset.Topic, offset.Partition.Value), out var offsets))
        {
            return;
        }

        lock (offsets)
        {
            if (offsets.ShouldUpdateOffset(offset.Offset.Value))
            {
                _committer.StoreOffset(
                    new TopicPartitionOffset(
                        offset.TopicPartition,
                        new Offset(offsets.LastOffset + 1)));
            }
        }
    }

    public void AddOffset(TopicPartitionOffset offset)
    {
        if (_partitionsOffsets.TryGetValue((offset.Topic, offset.Partition.Value), out var offsets))
        {
            offsets.AddOffset(offset.Offset.Value);
        }
    }
}