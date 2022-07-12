using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Erm.KafkaClient.Consumers.DistributionStrategies;

/// <summary>
///     This strategy chooses the first free worker to process the message. When a worker finishes the processing, it
///     notifies the worker pool that it is free to get a new message
///     This is the fastest and resource-friendly strategy (the message buffer is not used) but messages with the same
///     partition key can be delivered in different workers, so, no message order guarantee
/// </summary>
public class FreeWorkerDistributionStrategy : IDistributionStrategy
{
    private readonly Channel<IWorker> _freeWorkers = Channel.CreateUnbounded<IWorker>();

    /// <inheritdoc />
    public void Initialize(IReadOnlyList<IWorker> workers)
    {
        foreach (var worker in workers)
        {
            worker.OnTaskCompleted(() => _freeWorkers.Writer.WriteAsync(worker));
            _freeWorkers.Writer.TryWrite(worker);
        }
    }

    /// <inheritdoc />
    public Task<IWorker> GetWorker(IEnumerable<byte> partitionKey, CancellationToken cancellationToken)
    {
        return _freeWorkers.Reader.ReadAsync(cancellationToken).AsTask();
    }
}