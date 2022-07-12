using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Erm.KafkaClient;

internal class KafkaClientHostedService : IHostedService
{
    private readonly IKafkaClient _kafkaClient;

    public KafkaClientHostedService(IServiceProvider serviceProvider)
    {
        _kafkaClient = serviceProvider.CreateKafkaClient();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _kafkaClient.Start(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _kafkaClient.Stop();
    }
}