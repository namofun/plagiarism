using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend;
using Plag.Backend.Services;

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

            endpoints.MapApiDocument(
                name: "jplag",
                title: "JPlag Online",
                description: "Plagiarism detection API",
                version: "v2.12.2");
        }

        public override void RegisterServices(IServiceCollection services)
        {
            new TRole().Apply(services);
            services.EnsureScoped<IPlagiarismDetectService>();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(500)
                    .HasTitle(string.Empty, "Plagiarism Detect")
                    .HasLink("Dashboard", "Plagiarism", "List");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(250)
                    .HasTitle(string.Empty, "JPlag Online API")
                    .HasLink("/api/doc/jplag");
            });
        }
    }
}
