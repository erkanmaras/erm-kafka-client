using System.Collections.Generic;
using System.Linq;

namespace Erm.KafkaClient.Producers;

internal class ProducerAccessor : IProducerAccessor
{
    private readonly Dictionary<string, IKafkaProducer> _producers;

    public ProducerAccessor(IEnumerable<IKafkaProducer> producers)
    {
        _producers = producers.ToDictionary(x => x.ProducerName);
    }

    public IEnumerable<IKafkaProducer> All => _producers.Values;

    public IKafkaProducer this[string name] => GetProducer(name);

    public IKafkaProducer GetProducer(string name)
    {
        return _producers.TryGetValue(name, out var producer) ? producer : null;
    }

    public IKafkaProducer GetProducer<TProducer>()
    {
        return _producers.TryGetValue(typeof(TProducer).FullName!, out var producer) ? producer : null;
    }
}