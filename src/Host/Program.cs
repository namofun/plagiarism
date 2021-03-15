using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Plag.Backend;
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModuleIf<PlagModule.PlagModule<RestfulBackendRole>>(args.Contains("--restful"))
                .AddModuleIf<PlagModule.PlagModule<StorageBackendRole<DefaultContext>>>(!args.Contains("--restful"))
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaultsCore();
    }

    internal static class HostExt
    {
        public static IHostBuilder AddModuleIf<TModule>(this IHostBuilder host, bool condition) where TModule : AbstractModule, new()
        {
            if (condition) host.AddModule<TModule>();
            return host;
        }
    }
}
