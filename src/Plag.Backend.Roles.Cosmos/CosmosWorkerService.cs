using Plag.Backend.Models;
using Plag.Backend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plag.Backend
{
    public class CosmosWorkerService : IJobContext
    {
        public Task CompileAsync(string setid, int submitId, string error, byte[] result)
        {
            throw new NotImplementedException();
        }

        public Task<ReportTask> DequeueReportAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Submission> DequeueSubmissionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Submission> FindSubmissionAsync(string setId, int submitId, bool includeFiles = true)
        {
            throw new NotImplementedException();
        }

        public Task<Compilation> GetCompilationAsync(string setId, int submitId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<SubmissionFile>> GetFilesAsync(string setId, int submitId)
        {
            throw new NotImplementedException();
        }

        public Task SaveReportAsync(ReportTask task, ReportFragment fragment)
        {
            throw new NotImplementedException();
        }

        public Task ScheduleAsync(string setId, int submitId, int exclusive, int inclusive, string langId)
        {
            throw new NotImplementedException();
        }
    }
}
