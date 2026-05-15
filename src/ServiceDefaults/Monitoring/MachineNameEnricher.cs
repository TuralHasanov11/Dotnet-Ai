using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.Enrichment;

namespace ServiceDefaults.Monitoring;

public class MachineNameEnricher : IStaticLogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add("MachineName", Environment.MachineName);
    }
}