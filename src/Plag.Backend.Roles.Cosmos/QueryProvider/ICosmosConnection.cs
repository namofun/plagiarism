using Microsoft.Azure.Cosmos;
using Plag.Backend.Models;
using Plag.Backend.QueryProvider;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public interface ICosmosConnection
    {
        CosmosContainer<PlagiarismSet> Sets { get; }
        CosmosContainer<Entities.SubmissionEntity> Submissions { get; }
        CosmosContainer<Entities.ReportEntity> Reports { get; }
        CosmosContainer<Entities.MetadataEntity> Metadata { get; }

        Database GetDatabase();
        Task MigrateAsync();
    }
}
