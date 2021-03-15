using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Plag.Backend;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using System;
using System.Diagnostics;
using System.Linq;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
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
                host.AddModule<PlagModule.PlagModule<StorageBackendRole<DefaultContext>>>();

                host.AddDatabase<DefaultContext>((context, builder) =>
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

            if (Debugger.IsAttached)
            {
                host.AddModule<TelemetryModule.TelemetryModule>();
            }

            return host.ConfigureSubstrateDefaultsCore();
        }
    }
}
