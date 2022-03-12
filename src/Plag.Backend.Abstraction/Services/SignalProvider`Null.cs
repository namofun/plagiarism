using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    public class NullSignalProvider : ISignalProvider
    {
        public Task SendCompileSignalAsync()
        {
            return Task.CompletedTask;
        }

        public Task SendReportSignalAsync()
        {
            return Task.CompletedTask;
        }

        public Task SendRescueSignalAsync()
        {
            return Task.CompletedTask;
        }
    }
}
