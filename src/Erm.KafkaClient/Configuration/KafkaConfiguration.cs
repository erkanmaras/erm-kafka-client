using System.Collections.Generic;

namespace Erm.KafkaClient.Configuration;

internal class KafkaConfiguration
{
    private readonly List<ClusterConfiguration> _clusters = new();

    public IReadOnlyCollection<ClusterConfiguration> Clusters => _clusters;

    public void AddClusters(IEnumerable<ClusterConfiguration> configurations)
    {
        _clusters.AddRange(configurations);
    }
}