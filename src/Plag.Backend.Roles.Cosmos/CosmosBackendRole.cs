using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.QueryProvider;
using Plag.Backend.Services;

namespace Plag.Backend
{
    public class CosmosBackendRole : IBackendRoleStrategy
    {
        public void Apply(IServiceCollection services)
        {
            services.AddOptions<PlagBackendCosmosOptions>().PostConfigure(options => options.Validate());
            services.AddSingleton<ICosmosConnection, CosmosConnection>();
            services.AddScoped<IPlagiarismDetectService, CosmosStoreService>();
        }
    }
}
