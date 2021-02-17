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
            services.AddSingletonUpcast<IConvertService, IConvertService2>();
            services.AddSingleton<IConvertService2, DefaultConvertService2>();
            services.AddSingleton<IReportService, GstReportService>();

            services.AddScoped<IStoreExtService, EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IPlagiarismDetectService, IStoreExtService>();

            services.AddSingleton<IResettableSignalSource, ResettableSignalSource<SemaphoreSlimResettableSignal>>();
            services.Add(ServiceDescriptor.Singleton(typeof(IResettableSignal<>), typeof(TypedResettableSignal<>)));

            services.AddHostedService<SubmissionTokenizeService>();
            services.AddHostedService<ReportGenerationService>();

            services.AddDbModelSupplier<TContext, PlagEntityConfiguration<TContext>>();
        }
    }
}
