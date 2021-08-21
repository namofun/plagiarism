using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SatelliteSite
{
    public class HostModule : AbstractModule
    {
        public override string Area => null;

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            var app = endpoints.CreateApplicationBuilder();

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "../React.UI";

                if (endpoints.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            endpoints.MapFallback("/{**slug}", app.Build())
                .WithDisplayName("Fallback React UI");
        }
    }
}
