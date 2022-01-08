using Microsoft.Azure.Cosmos;
using Plag.Backend.Entities;
using Plag.Backend.QueryProvider;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public interface ICosmosConnection
    {
        CosmosContainer<SetEntity> Sets { get; }
        CosmosContainer<SubmissionEntity> Submissions { get; }
        CosmosContainer<ReportEntity> Reports { get; }
        CosmosContainer<MetadataEntity> Metadata { get; }

        Database GetDatabase();
        Task MigrateAsync();
    }
}
