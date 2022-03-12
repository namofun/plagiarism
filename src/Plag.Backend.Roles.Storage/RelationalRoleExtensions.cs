using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend
{
    public static class RelationalRoleExtensions
    {
        /// <summary>
        /// Configures the cosmos db role for functional worker.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">The connection configurator.</param>
        public static void AddRelationalForPlagWorker<TContext>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureOptions)
            where TContext : DbContext
        {
            services.AddSingleton<ISignalProvider, NullSignalProvider>();
            services.AddScoped<EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IPlagiarismDetectService, EntityFrameworkCoreStoreService<TContext>>();
            services.AddScopedUpcast<IJobContext, EntityFrameworkCoreStoreService<TContext>>();

            services.AddScoped<IConfigurationRegistry, ConfigurationRegistry<TContext>>();
            services.AddDbContext<TContext>(
                optionsLifetime: ServiceLifetime.Singleton,
                optionsAction: (serviceProvider, options) =>
                {
                    ModelSupplierService<TContext> mss = new(new IDbModelSupplier<TContext>[]
                    {
                        new PlagEntityConfiguration<TContext>(),
                        new ConfigurationEntityConfiguration<TContext>(),
                    });

                    options.UseApplicationServiceProvider(null);
                    options.UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>());

                    ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(mss);
                    configureOptions.Invoke(options);

                    bool bulkRegistered = false;
                    foreach (var ext in options.Options.Extensions.ToList())
                    {
                        if (ext is Microsoft.EntityFrameworkCore.Bulk.BatchOptionsExtension)
                        {
                            bulkRegistered = true;
                        }
                        else if (ext is RelationalOptionsExtension relExt && ext.GetType().FullName != "Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension")
                        {
                            options.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.DiagnosticDbInterceptor.Instance);
                        }
                    }

                    if (!bulkRegistered)
                    {
                        throw new InvalidOperationException(
                            "Extensions for Microsoft.EntityFrameworkCore.Bulk hasn't been registered. " +
                            "Please register it with options.UseSqlServer(..., b => b.UseBulk()).");
                    }
                });
        }

        private class ConfigurationEntityConfiguration<TContext> :
            EntityTypeConfigurationSupplier<TContext>,
            IEntityTypeConfiguration<Configuration>
            where TContext : DbContext
        {
            public void Configure(EntityTypeBuilder<Configuration> entity)
            {
                entity.HasKey(e => e.Name);

                entity.ToTable("Configurations");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false)
                    .HasMaxLength(128);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired();
            }
        }

        private class ConfigurationRegistry<TContext> : IConfigurationRegistry
            where TContext : DbContext
        {
            public TContext Context { get; }

            DbSet<Configuration> Configurations => Context.Set<Configuration>();

            public ConfigurationRegistry(TContext context)
            {
                Context = context;
            }

            public async Task<bool> UpdateAsync(string name, string newValue)
            {
                var result = await Configurations
                    .Where(c => c.Name == name)
                    .BatchUpdateAsync(c => new Configuration { Value = newValue });

                return result == 1;
            }

            public async Task<Configuration> FindAsync(string config)
            {
                return await Configurations.FindAsync(config);
            }

            public async Task<List<Configuration>> GetAsync(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return await Configurations.ToListAsync();
                }

                var item = await FindAsync(name);
                if (item == null)
                {
                    return new List<Configuration>(0);
                }
                else
                {
                    return new List<Configuration>(1) { item };
                }
            }

            public async Task<ILookup<string, Configuration>> ListAsync()
            {
                var conf = await Configurations
                    .Where(c => c.Public)
                    .ToListAsync();

                return conf.OrderBy(c => c.DisplayPriority)
                    .ToLookup(c => c.Category);
            }

            private async Task<T> GetValueAsync<T>(string name, string typeName)
            {
                var conf = await FindAsync(name);

                if (conf == null)
                {
                    throw new KeyNotFoundException(
                        $"The configuration {name} is not saved. Please check your migration status.");
                }
                else if (conf.Type != typeName)
                {
                    throw new InvalidCastException(
                        $"The type of configuration {name} is not correct.");
                }

                return conf.Value.AsJson<T>();
            }

            public Task<bool?> GetBooleanAsync(string name)
            {
                return GetValueAsync<bool?>(name, "bool");
            }

            public Task<int?> GetIntegerAsync(string name)
            {
                return GetValueAsync<int?>(name, "int");
            }

            public Task<DateTimeOffset?> GetDateTimeOffsetAsync(string name)
            {
                return GetValueAsync<DateTimeOffset?>(name, "datetime");
            }

            public Task<string> GetStringAsync(string name)
            {
                return GetValueAsync<string>(name, "string");
            }
        }
    }
}
