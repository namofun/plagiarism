using Microsoft.Extensions.DependencyInjection;

namespace Xylab.PlagiarismDetect.Backend
{
    /// <summary>
    /// Configurator for backend services.
    /// </summary>
    public interface IBackendRoleStrategy
    {
        /// <summary>
        /// Apply services for backend role server.
        /// </summary>
        /// <param name="services">The dependency injection container builder.</param>
        void Apply(IServiceCollection services);
    }
}
