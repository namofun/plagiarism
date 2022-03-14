using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend.Jobs
{
    /// <summary>
    /// The service graph fixing core logic.
    /// </summary>
    public static class GraphRecoveryWorker
    {
        public static async Task<bool> RunAsync(
            string setId,
            bool forced,
            IJobContext store,
            ILogger log)
        {
            if (!store.SupportServiceGraph)
            {
                log.LogInformation("Current storage provider doesn't support service graph.");
                return false;
            }

            log.LogInformation("Start graph recovery.");

            IServiceGraphContext context = (IServiceGraphContext)store;
            PlagiarismSet set = await context.FindSetAsync(setId);
            if (set == null)
            {
                log.LogError("No such set found, stop processing.");
                return false;
            }

            List<ServiceVertex> vertices = await context.GetVerticesAsync(set);
            ILookup<(int, string), ServiceVertex> cliques = vertices.ToLookup(v => (v.Inclusive, v.Language));

            long expectedEdgeCount = 0;
            foreach (IGrouping<(int, string), ServiceVertex> clique in cliques)
            {
                int cliqueNumber = clique.Count();
                expectedEdgeCount += cliqueNumber * (cliqueNumber - 1) / 2;
                foreach (IGrouping<int, ServiceVertex> redundantGroup in clique.GroupBy(c => c.Exclusive))
                {
                    int groupCount = redundantGroup.Count();
                    expectedEdgeCount -= groupCount * (groupCount - 1) / 2;
                }
            }

            if (expectedEdgeCount == set.ReportCount)
            {
                log.LogInformation("Edge count in graph {setId} is correct ({count}).", setId, set.ReportCount);
                if (!forced) return false;
            }
            else
            {
                log.LogInformation("Edge count in this graph {setId} is incorrect, expected {expected}, actually {actual}.", setId, expectedEdgeCount, set.ReportCount);
            }

            HashSet<ServiceEdge> visited = new(await context.GetEdgesAsync(set), new EdgeComparer());
            List<(ServiceVertex, ServiceVertex)> lostEdges = new();
            foreach (IGrouping<(int, string), ServiceVertex> clique in cliques)
            {
                List<ServiceVertex> cliqueList = clique.ToList();
                for (int i = 0; i < cliqueList.Count; i++)
                {
                    for (int j = i + 1; j < cliqueList.Count; j++)
                    {
                        if (cliqueList[i].Exclusive == cliqueList[j].Exclusive)
                        {
                            continue;
                        }

                        ServiceEdge edge = new() { U = cliqueList[i].Id, V = cliqueList[j].Id };
                        if (!visited.Contains(edge))
                        {
                            lostEdges.Add((cliqueList[i], cliqueList[j]));
                            log.LogDebug("Lost edge ({u}, {v}) detected.", edge.U, edge.V);
                        }
                    }
                }
            }

            await context.FixEdgesAsync(set, lostEdges);
            log.LogInformation("Edge fixed for {set}.", set.Id);
            return true;
        }

        private class EdgeComparer : IEqualityComparer<ServiceEdge>
        {
            public bool Equals(ServiceEdge x, ServiceEdge y)
            {
                return x != null && y != null &&
                    ((x.U == y.U && x.V == y.V)
                    || (x.V == y.U && x.U == y.V));
            }

            public int GetHashCode([DisallowNull] ServiceEdge obj)
            {
                var (u, v) = obj.U > obj.V ? (obj.U, obj.V) : (obj.V, obj.U);
                return HashCode.Combine(u, v);
            }
        }
    }
}
