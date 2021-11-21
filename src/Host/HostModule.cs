using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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

            var requestDelegate = app.Build();

            endpoints.MapFallback("/{**slug}", requestDelegate)
                .WithDisplayName("Fallback React UI");
            endpoints.MapFallback("/static/{**slug}", requestDelegate)
                .WithDisplayName("Fallback React UI")
                .Add(eb => ((RouteEndpointBuilder)eb).Order = -10);
        }
    }
}
