using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SatelliteSite.PlagModule
{
    public class PlagModule : AbstractModule
    {
        public override string Area => "Plagiarism";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<>
        }
    }
}
