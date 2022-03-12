using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Backend.Services
{
    /// <summary>
    /// The provider for sending out signals.
    /// </summary>
    public interface ISignalProvider
    {
        /// <summary>
        /// Sends rescue signal.
        /// </summary>
        Task SendRescueSignalAsync();

        /// <summary>
        /// Sends compile signal.
        /// </summary>
        Task SendCompileSignalAsync();

        /// <summary>
        /// Sends report signal.
        /// </summary>
        Task SendReportSignalAsync();
    }
}
