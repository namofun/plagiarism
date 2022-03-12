using Microsoft.Extensions.DependencyInjection;
using Xylab.PlagiarismDetect.Backend.QueryProvider;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend
{
    public class CosmosBackendRole : IBackendRoleStrategy
    {
        public void Apply(IServiceCollection services)
        {
            services.AddOptions<PlagBackendCosmosOptions>().PostConfigure(options => options.Validate());
            services.AddSingleton<ICosmosConnection, CosmosConnection>();
            services.AddScoped<CosmosStoreService>();
            services.AddScopedUpcast<IPlagiarismDetectService, CosmosStoreService>();
            services.AddScopedUpcast<IJobContext, CosmosStoreService>();
        }
    }
}
