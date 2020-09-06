using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend;
using Plag.Backend.Services;
using System;
using System.Linq;

namespace SatelliteSite.PlagModule
{
    public class PlagModule<TRole> : AbstractModule
        where TRole : class, IBackendRoleStrategy, new()
    {
        public override string Area => "Plagiarism";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.MapApiDocument("plag", "JPlag Online", "Plagiarism detection API", "v2.12.1");
        }

        public override void RegisterServices(IServiceCollection services)
        {
            new TRole().Apply(services);

            var cnt = services
                .Where(s => s.ServiceType == typeof(IStoreService))
                .Count();
            if (cnt == 0) throw new InvalidOperationException("No IStoreService injected.");
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(100)
                    .HasTitle(string.Empty, "Plagiarism Detect")
                    .HasLink("Dashboard", "Plagiarism", "List");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(55)
                    .HasTitle(string.Empty, "JPlag Online API")
                    .HasLink("/api/doc/plag");
            });
        }
    }
}
