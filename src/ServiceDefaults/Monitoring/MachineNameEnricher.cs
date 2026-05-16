using Microsoft.Extensions.Diagnostics.Enrichment;

namespace ServiceDefaults.Monitoring;

public class MachineNameEnricher : IStaticLogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add("MachineName", Environment.MachineName);
    }
}