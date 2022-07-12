using System.Threading;
using System.Threading.Tasks;
using Erm.KafkaClient.Consumers;
using Erm.KafkaClient.Producers;

namespace Erm.KafkaClient;

/// <summary>
///     Provides access to the kafka bus operations
/// </summary>
public interface IKafkaClient
{
    /// <summary>
    ///     Gets all configured consumers
    /// </summary>
    IConsumerAccessor Consumers { get; }

    /// <summary>
    ///     Gets all configured producers
    /// </summary>
    IProducerAccessor Producers { get; }

    /// <summary>
    ///     Starts all consumers
    /// </summary>
    /// <param name="stopCancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to stop the operation.</param>
    /// <returns></returns>
    Task Start(CancellationToken stopCancellationToken = default);

    /// <summary>
    ///     Stops all consumers
    /// </summary>
    /// <returns></returns>
    Task Stop();
}