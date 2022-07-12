using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Erm.KafkaClient.Consumers;

internal class PartitionOffsets
{
    private readonly SortedSet<long> _pendingOffsets = new();
    private readonly LinkedList<long> _offsetsOrder = new();

    public long LastOffset { get; private set; } = -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOffset(long offset)
    {
        lock (_offsetsOrder)
        {
            _offsetsOrder.AddLast(offset);
        }
    }

    public bool ShouldUpdateOffset(long newOffset)
    {
        lock (_offsetsOrder)
        {
            if (!_offsetsOrder.Any())
            {
                throw new InvalidOperationException(
                    $"There is no offsets in the queue. Call {nameof(AddOffset)} first");
            }

            if (newOffset != _offsetsOrder.First!.Value)
            {
                _pendingOffsets.Add(newOffset);
                return false;
            }

            do
            {
                LastOffset = _offsetsOrder.First.Value;
                _offsetsOrder.RemoveFirst();
            }
            while (_offsetsOrder.Count > 0 && _pendingOffsets.Remove(_offsetsOrder.First!.Value));
        }

        return true;
    }
}