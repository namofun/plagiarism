using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plag.Backend;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();

            if (args.Contains("--restful"))
            {
                // no database are needed.
            }
            else if (args.Contains("--production"))
            {
                Current.EnsureCreated<ProductionContext>();
                Current.AutoMigrate<ProductionContext>();
            }
            else
            {
                Current.AutoMigrate<DevelopmentContext>();
            }

            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args);
            host.MarkDomain<Program>();

            if (args.Contains("--restful"))
            {
                host.AddModule<PlagModule.PlagModule<RestfulBackendRole>>();
            }
            else
            {
                void ConfigureDatabase<TContext>() where TContext : DbContext
                {
                    host.AddModule<PlagModule.PlagModule<StorageBackendRole<TContext>>>();

                    host.AddDatabase<TContext>((context, builder) =>
                    {
                        var connectionString = context.GetConnectionString("UserDbConnection");
                        var connectionType = context.GetConnectionString("UserDbConnectionType");

                        if (connectionString == null)
                        {
                            throw new ArgumentException("Please specify the database connection string by UserDbConnection.");
                        }

                        if (connectionType == "SqlServer")
                        {
                            builder.UseSqlServer(connectionString, b => b.UseBulk());
                        }
                        else if (connectionType == "Npgsql")
                        {
                            builder.UseNpgsql(connectionString, b => b.UseBulk());
                        }
                        else if (connectionType == "MySql")
                        {
                            var version = ServerVersion.AutoDetect(connectionString);
                            builder.UseMySql(connectionString, b => b.ServerVersion(version).UseBulk());
                        }
                        else
                        {
                            throw new ArgumentException("No database specified or not support. Please choose from \"SqlServer\", \"Npgsql\", \"MySql\".");
                        }
                    });
                }

                if (args.Contains("--production"))
                {
                    // For production use, the migration is pre-configured.
                    ConfigureDatabase<ProductionContext>();
                }
                else
                {
                    // In this case, we can add migrations for development use.
                    ConfigureDatabase<DevelopmentContext>();
                }
            }

            if (Debugger.IsAttached)
            {
                host.AddModule<TelemetryModule.TelemetryModule>();
            }

            if (args.Contains("--production"))
            {
                host.ConfigureServices(services =>
                {
                    services.ConfigureApplicationBuilder(options =>
                    {
                        options.PointBeforeEndpoint.Add(app =>
                        {
                            app.Use(FakeAuthorization);
                        });
                    });
                });

                static Task FakeAuthorization(HttpContext httpContext, Func<Task> next)
                {
                    httpContext.Items["__AuthorizationMiddlewareWithEndpointInvoked"] = true;
                    return next();
                }
            }

            return host.ConfigureSubstrateDefaultsCore();
        }
    }
}
