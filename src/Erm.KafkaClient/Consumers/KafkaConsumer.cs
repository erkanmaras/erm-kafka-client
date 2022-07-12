using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Consumers;

internal class KafkaConsumer : IKafkaConsumer
{
    private readonly IConsumerManager _consumerManager;
    private readonly ILogger<KafkaConsumer> _logger;

    public KafkaConsumer(IConsumerManager consumerManager, ILogger<KafkaConsumer> logger)
    {
        _consumerManager = consumerManager;
        _logger = logger;
    }

    public string ConsumerName => _consumerManager.Consumer.Configuration.ConsumerName;

    public string GroupId => _consumerManager.Consumer.Configuration.GroupId;

    public IReadOnlyList<string> Subscription => _consumerManager.Consumer.Subscription;

    public IReadOnlyList<TopicPartition> Assignment => _consumerManager.Consumer.Assignment;

    public string MemberId => _consumerManager.Consumer.MemberId;

    public string ClientInstanceName => _consumerManager.Consumer.ClientInstanceName;

    public int WorkerCount => _consumerManager.Consumer.Configuration.WorkerCount;

    public Offset GetPosition(TopicPartition topicPartition)
    {
        return _consumerManager.Consumer.GetPosition(topicPartition);
    }

    public async Task OverrideOffsetsAndRestart(IReadOnlyCollection<TopicPartitionOffset> offsets)
    {
        if (offsets is null)
        {
            throw new ArgumentNullException(nameof(offsets));
        }

        try
        {
            await _consumerManager.Feeder.Stop().ConfigureAwait(false);
            await _consumerManager.WorkerPool.Stop().ConfigureAwait(false);

            _consumerManager.Consumer.Commit(offsets);

            await InternalRestart().ConfigureAwait(false);

            _logger.LogInformation("Kafka offsets overridden {Consumer} {OffsetLog}", ConsumerName, GetOffsetsLogData(offsets));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error overriding offsets {Topic}", offsets.First().Topic);
            throw;
        }
    }

    public async Task Restart()
    {
        await InternalRestart().ConfigureAwait(false);
        _logger.LogInformation("Erm.KafkaClient consumer manually restarted");
    }

    private async Task InternalRestart()
    {
        await _consumerManager.Stop().ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        await _consumerManager.Start().ConfigureAwait(false);
    }

    private static object GetOffsetsLogData(IEnumerable<TopicPartitionOffset> offsets) => offsets
        .GroupBy(x => x.Topic)
        .Select(
            x => new
            {
                x.First().Topic,
                Partitions = JsonSerializer.Serialize(x.Select(
                    y => new
                    {
                        Partition = y.Partition.Value,
                        Offset = y.Offset.Value
                    }).ToList()),
            });
}