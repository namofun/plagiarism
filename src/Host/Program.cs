using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>();

            if (args.Contains("--restful"))
                host.AddModule<PlagModule.PlagModule<RestfulBackendRole>>();
            else
                host.AddModule<PlagModule.PlagModule<StorageBackendRole<DefaultContext>>>();

            host.AddDatabaseMssql<DefaultContext>("UserDbConnection")
                .ConfigureSubstrateDefaults<DefaultContext>();
            return host;
        }
    }
}
