using Microsoft.Azure.Cosmos;
using Xylab.PlagiarismDetect.Backend.Entities;
using Xylab.PlagiarismDetect.Backend.QueryProvider;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend
{
    public interface ICosmosConnection
    {
        CosmosContainer<SetEntity> Sets { get; }
        CosmosContainer<SubmissionEntity> Submissions { get; }
        CosmosContainer<ReportEntity> Reports { get; }
        CosmosContainer<MetadataEntity> Metadata { get; }
        Task MigrateAsync();
    }
}
