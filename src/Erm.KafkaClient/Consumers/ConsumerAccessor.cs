using System.Collections.Generic;

namespace Erm.KafkaClient.Consumers;

internal class ConsumerAccessor : IConsumerAccessor
{
    private readonly IDictionary<string, IKafkaConsumer> _consumers = new Dictionary<string, IKafkaConsumer>();

    public IEnumerable<IKafkaConsumer> All => _consumers.Values;

    public IKafkaConsumer this[string name] => GetConsumer(name);

    public IKafkaConsumer GetConsumer(string name)
    {
        return _consumers.TryGetValue(name, out var consumer) ? consumer : null;
    }

    void IConsumerAccessor.Add(IKafkaConsumer kafkaConsumer)
    {
        _consumers.Add(kafkaConsumer.ConsumerName, kafkaConsumer);
    }
}