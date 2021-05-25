using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Jobs;
using Plag.Backend.Services;

namespace Plag.Backend
{
    public class StorageBackendRole<TContext> : IBackendRoleStrategy
        where TContext : DbContext
    {
        public void Apply(IServiceCollection services)
        {
            services.AddSingleton<ICompileService, AntlrCompileService>();
            services.AddSingletonUpcast<IConvertService, IConvertService2>();
            services.AddSingleton<IConvertService2, DefaultConvertService2>();
            services.AddSingleton<IReportService, GstReportService>();

            services.AddScoped<EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IPlagiarismDetectService, EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IJobContext, EntityFrameworkCoreStoreService<TContext>>();

            services.AddHostedService<SubmissionTokenizeService>();
            services.AddHostedService<ReportGenerationService>();

            services.AddDbModelSupplier<TContext, PlagEntityConfiguration<TContext>>();
        }
    }
}
