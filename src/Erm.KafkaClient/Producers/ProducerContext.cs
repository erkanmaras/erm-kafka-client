namespace Erm.KafkaClient.Producers;

internal class ProducerContext : IProducerContext
{
    public ProducerContext(string topic)
    {
        Topic = topic;
    }

    public string Topic { get; }

    public int? Partition { get; set; }

    public long? Offset { get; set; }
}