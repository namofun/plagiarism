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
        CosmosContainer<LanguageInfo> Languages { get; }
        CosmosContainer<Entities.ReportEntity> Reports { get; }

        Database GetDatabase();
        Task MigrateAsync();
    }
}
