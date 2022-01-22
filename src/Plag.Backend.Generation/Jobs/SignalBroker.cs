using Plag.Backend.Services;
using System.Threading.Tasks;

namespace Plag.Backend.Jobs
{
    public interface ISignalBroker
    {
        Task FireAsync(string signal);
    }

    public class BrokeredSignalProvider : ISignalProvider
    {
        public ISignalBroker Compile { get; set; }

        public ISignalBroker Report { get; set; }

        public Task SendCompileSignalAsync()
        {
            if (Compile == null)
            {
                return Task.CompletedTask;
            }
            else
            {
                string signal = CorrelationRecord.New("schedule");
                return Compile.FireAsync(signal);
            }
        }

        public Task SendReportSignalAsync()
        {
            if (Report == null)
            {
                return Task.CompletedTask;
            }
            else
            {
                string signal = CorrelationRecord.New("schedule");
                return Report.FireAsync(signal);
            }
        }

        public async Task SendRescueSignalAsync()
        {
            string signal = CorrelationRecord.New("rescue");
            if (Compile != null) await Compile.FireAsync(signal);
            if (Report != null) await Report.FireAsync(signal);
        }
    }
}
