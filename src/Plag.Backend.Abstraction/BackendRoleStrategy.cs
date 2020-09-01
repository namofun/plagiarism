using Microsoft.Extensions.DependencyInjection;

namespace Plag.Backend
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
