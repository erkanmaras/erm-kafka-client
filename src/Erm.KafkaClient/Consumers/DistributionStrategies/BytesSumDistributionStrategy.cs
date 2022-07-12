using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Erm.KafkaClient.Consumers.DistributionStrategies;

/// <summary>
///     This strategy sums all bytes in the partition key and apply a mod operator with the total number of workers, the
///     resulting number is the worker ID to be chosen
///     This algorithm is fast and creates a good work balance. Messages with the same partition key are always delivered
///     in the same worker, so, message order is guaranteed
///     Set an optimal message buffer value to avoid idle workers (it will depends how many messages with the same
///     partition key are consumed)
/// </summary>
public class BytesSumDistributionStrategy : IDistributionStrategy
{
    private IReadOnlyList<IWorker> _workers;

    /// <inheritdoc />
    public void Initialize(IReadOnlyList<IWorker> workers)
    {
        _workers = workers;
    }

    /// <inheritdoc />
    public Task<IWorker> GetWorker(IEnumerable<byte> partitionKey, CancellationToken cancellationToken)
    {
        if (partitionKey is null) return Task.FromResult(_workers[0]);

        return cancellationToken.IsCancellationRequested ? null : Task.FromResult(_workers.ElementAtOrDefault(partitionKey.Sum(x => x) % _workers.Count));
    }
}