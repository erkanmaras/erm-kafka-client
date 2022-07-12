using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Erm.KafkaClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Erm.KafkaClient.Producers;

internal class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducerConfiguration _configuration;
    private readonly object _producerCreationSync = new();
    private readonly IServiceScope _serviceScope;
    private readonly ILogger<KafkaProducer> _logger;
    private volatile IProducer<byte[], byte[]> _producer;

    public KafkaProducer(IServiceProvider serviceProvider, IProducerConfiguration configuration)
    {
        _configuration = configuration;
        // Create middlewares instances inside a scope to allow scoped injections in producer middlewares
        _serviceScope = serviceProvider.CreateScope();
        _logger = _serviceScope.ServiceProvider.GetRequiredService<ILogger<KafkaProducer>>();
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _producer?.Dispose();
    }

    public string ProducerName => _configuration.Name;

    public async Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
        string topic,
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null)
    {
        return await InternalProduceAsync(
            new KafkaMessageContext(
                new KafkaMessage(messageKey, messageValue),
                headers,
                null,
                new ProducerContext(topic))).ConfigureAwait(false);
    }

    public Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null)
    {
        if (string.IsNullOrWhiteSpace(_configuration.DefaultTopic))
        {
            throw new InvalidOperationException($"There is no default topic defined for producer {ProducerName}");
        }

        return ProduceAsync(
            _configuration.DefaultTopic,
            messageKey,
            messageValue,
            headers);
    }

    public void Produce(
        string topic,
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null,
        Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null)
    {
        InternalProduce(
            new KafkaMessageContext(new KafkaMessage(messageKey, messageValue), headers, null, new ProducerContext(topic)),
            report => { deliveryHandler?.Invoke(report); });
    }

    public void Produce(
        byte[] messageKey,
        byte[] messageValue,
        IKafkaMessageHeaders headers = null,
        Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null)
    {
        if (string.IsNullOrWhiteSpace(_configuration.DefaultTopic))
        {
            throw new InvalidOperationException($"There is no default topic defined for producer {ProducerName}");
        }

        Produce(_configuration.DefaultTopic, messageKey, messageValue, headers, deliveryHandler);
    }

    private static void FillContextWithResultMetadata(IKafkaMessageContext context, DeliveryResult<byte[], byte[]> result)
    {
        var concreteProducerContext = (ProducerContext)context.ProducerContext;
        concreteProducerContext.Offset = result.Offset;
        concreteProducerContext.Partition = result.Partition;
    }

    private static Message<byte[], byte[]> CreateMessage(IKafkaMessageContext context)
    {
        return new Message<byte[], byte[]>
        {
            Key = context.KafkaMessage.Key,
            Value = context.KafkaMessage.Value,
            Headers = ((KafkaMessageHeaders)context.Headers).GetKafkaHeaders(),
            Timestamp = Timestamp.Default
        };
    }

    private IProducer<byte[], byte[]> EnsureProducer()
    {
        lock (_producerCreationSync)
        {
            if (_producer != null)
            {
                return _producer;
            }

            var producerBuilder = new ProducerBuilder<byte[], byte[]>(_configuration.GetKafkaConfig())
                .SetErrorHandler(
                    (_, error) =>
                    {
                        if (error.IsFatal)
                        {
                            InvalidateProducer(error);
                        }
                        else
                        {
                            _logger.LogError("Kafka Producer Error: {Error}", error.ToString());
                        }
                    })
                .SetStatisticsHandler(
                    (_, statistics) => { _configuration.StatisticsHandler.Invoke(statistics); });

            return _producer = _configuration.Factory(producerBuilder.Build(), _serviceScope.ServiceProvider);
        }
    }

    private void InvalidateProducer(Error error)
    {
        lock (_producerCreationSync)
        {
            _producer = null;
            _logger.LogError("Kafka produce fatal error occurred. The producer will be recreated Error : {Error}", error.ToString());
        }
    }

    private async Task<DeliveryResult<byte[], byte[]>> InternalProduceAsync(IKafkaMessageContext context)
    {
        DeliveryResult<byte[], byte[]> result;

        try
        {
            result = await EnsureProducer().ProduceAsync(context.ProducerContext.Topic, CreateMessage(context)).ConfigureAwait(false);
        }
        catch (ProduceException<byte[], byte[]> e)
        {
            if (e.Error.IsFatal)
            {
                InvalidateProducer(e.Error);
            }

            throw;
        }

        FillContextWithResultMetadata(context, result);

        return result;
    }

    private void InternalProduce(IKafkaMessageContext context, Action<DeliveryReport<byte[], byte[]>> deliveryHandler)
    {
        EnsureProducer().Produce(context.ProducerContext.Topic, CreateMessage(context),
            report =>
            {
                if (report.Error.IsFatal)
                {
                    InvalidateProducer(report.Error);
                }

                FillContextWithResultMetadata(context, report);
                deliveryHandler(report);
            });
    }
}