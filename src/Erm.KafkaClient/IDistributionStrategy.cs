using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Erm.KafkaClient;

/// <summary>
///     An interface used to create a distribution strategy
/// </summary>
public interface IDistributionStrategy
{
    /// <summary>
    ///     Initializes the distribution strategy, this method is called when a consumer is started
    /// </summary>
    /// <param name="workers">List of workers to be initialized</param>
    void Initialize(IReadOnlyList<IWorker> workers);

    /// <summary>
    ///     Gets an available worker to process the message
    /// </summary>
    /// <param name="partitionKey">Message partition key</param>
    /// <param name="cancellationToken">
    ///     A <see cref="T:System.Threading.CancellationToken" /> that is cancelled when the
    ///     consumers stops
    /// </param>
    /// <returns></returns>
    Task<IWorker> GetWorker(IEnumerable<byte> partitionKey, CancellationToken cancellationToken);
}