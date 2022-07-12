using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class ConsumerWorkerPool : IConsumerWorkerPool
{
    private readonly IConsumer _consumer;
    private readonly Factory<IDistributionStrategy> _distributionStrategyFactory;
    private readonly IConsumerMessageHandler _consumerMessageHandler;
    private readonly IServiceProvider _serviceProvider;
    private IDistributionStrategy _distributionStrategy;
    private OffsetManager _offsetManager;
    private List<IConsumerWorker> _workers = new();

    public ConsumerWorkerPool(
        IConsumer consumer,
        IServiceProvider serviceProvider,
        IConsumerMessageHandler consumerMessageHandler,
        Factory<IDistributionStrategy> distributionStrategyFactory)
    {
        _consumer = consumer;
        _serviceProvider = serviceProvider;
        _consumerMessageHandler = consumerMessageHandler;
        _distributionStrategyFactory = distributionStrategyFactory;
    }

    public async Task Start(IEnumerable<TopicPartition> partitions)
    {
        _offsetManager = new OffsetManager(new OffsetCommitter(_consumer, _serviceProvider.GetService<ILogger<OffsetCommitter>>()), partitions);

        for (var workerId = 0; workerId < _consumer.Configuration.WorkerCount; workerId++)
        {
            _workers.Add(new ConsumerWorker(_consumer, workerId, _offsetManager, _serviceProvider.GetService<ILogger<ConsumerWorker>>(), _consumerMessageHandler));
        }

        await Task.WhenAll(_workers.Select(worker => worker.Start())).ConfigureAwait(false);

        _distributionStrategy = _distributionStrategyFactory(_serviceProvider);
        _distributionStrategy.Initialize(_workers.AsReadOnly());
    }

    public async Task Stop()
    {
        var currentWorkers = _workers;
        _workers = new List<IConsumerWorker>();
        await Task.WhenAll(currentWorkers.Select(x => x.Stop())).ConfigureAwait(false);
        _offsetManager?.Dispose();
        _offsetManager = null;
    }

    public async Task Enqueue(ConsumeResult<byte[], byte[]> message, CancellationToken stopCancellationToken)
    {
        var localOffsetManager = _offsetManager;
        var worker = (IConsumerWorker)await _distributionStrategy.GetWorker(message.Message.Key, stopCancellationToken).ConfigureAwait(false);

        if (worker is null || localOffsetManager is null)
        {
            return;
        }

        localOffsetManager.AddOffset(message.TopicPartitionOffset);

        await worker
            .Enqueue(message, stopCancellationToken)
            .ConfigureAwait(false);
    }
}