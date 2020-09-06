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

            services.AddScoped<IStoreExtService, EntityFrameworkCoreStoreService<TContext>>();
            services.AddScoped<IStoreService>(sp => sp.GetRequiredService<IStoreExtService>());

            services.AddHostedService<SubmissionTokenizeService>();
            services.AddHostedService<ReportGenerationService>();

            services.AddDbModelSupplier<TContext, PlagEntityConfiguration<TContext>>();
        }
    }
}
