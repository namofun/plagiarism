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
            services.AddScoped<EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IPlagiarismDetectService, EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IJobContext, EntityFrameworkCoreStoreService<TContext>>();

            services.AddDbModelSupplier<TContext, PlagEntityConfiguration<TContext>>();
        }
    }
}
