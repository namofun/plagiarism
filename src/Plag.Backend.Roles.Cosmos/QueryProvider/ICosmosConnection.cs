using System.Threading.Tasks;
using Xylab.DataAccess.Cosmos;
using Xylab.PlagiarismDetect.Backend.Entities;

namespace Xylab.PlagiarismDetect.Backend
{
    public interface ICosmosConnection
    {
        Container<SetEntity> Sets { get; }
        Container<SubmissionEntity> Submissions { get; }
        Container<ReportEntity> Reports { get; }
        Container<MetadataEntity> Metadata { get; }
        Task MigrateAsync();
    }
}
