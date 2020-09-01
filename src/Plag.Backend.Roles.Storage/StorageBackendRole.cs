using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Services;

namespace Plag.Backend
{
    public class StorageBackendRole<TContext> : IBackendRoleStrategy
        where TContext : DbContext
    {
        public void Apply(IServiceCollection services)
        {
            services.AddSingleton<ICompileService, AntlrCompileService>();
            services.AddSingleton<IConvertService>(sp => sp.GetRequiredService<IConvertService2>());
            services.AddSingleton<IConvertService2, DefaultConvertService2>();
            services.AddSingleton<IReportService, GstReportService>();

            services.AddScoped<IStoreService, EntityFrameworkCoreStoreService<TContext>>();
        }
    }
}
