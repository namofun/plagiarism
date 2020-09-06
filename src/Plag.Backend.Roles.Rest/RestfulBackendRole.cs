using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Services;

namespace Plag.Backend
{
    public class RestfulBackendRole : IBackendRoleStrategy
    {
        public void Apply(IServiceCollection services)
        {
            services.AddSingleton<IStoreService, RestfulStoreService>();
        }
    }
}
