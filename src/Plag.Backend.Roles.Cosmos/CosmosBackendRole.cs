using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Services;

namespace Plag.Backend
{
    public class CosmosBackendRole : IBackendRoleStrategy
    {
        public void Apply(IServiceCollection services)
        {
            services.AddOptions<PdsCosmosOptions>().PostConfigure(options => options.Validate());
            services.AddSingleton<ICosmosConnection, CosmosConnection>();
            services.AddScoped<IPlagiarismDetectService, CosmosStoreService>();
        }
    }
}
