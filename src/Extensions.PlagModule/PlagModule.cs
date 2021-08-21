using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend;
using Plag.Backend.Services;

[assembly: SatelliteSite.RoleDefinition(37, "PlagUser", "plaguser", "Plagiarism Detect User")]

namespace SatelliteSite.PlagModule
{
    public class PlagModule<TRole> : AbstractModule, IAuthorizationPolicyRegistry
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

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            new TRole().Apply(services);
            services.EnsureScoped<IPlagiarismDetectService>();

            if (configuration.GetValue<bool>("SatelliteSite:ApiOnly"))
            {
                services.AddSingleton(
                    new FeatureAvailabilityConvention(
                        false,
                        typeof(Controllers.ReportController),
                        typeof(Dashboards.PlagiarismController)));
            }
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(500)
                    .HasTitle(string.Empty, "Plagiarism Detect")
                    .HasLink("Dashboard", "Plagiarism", "List")
                    .RequireRoles("Administrator,PlagUser");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(250)
                    .HasTitle(string.Empty, "JPlag Online API")
                    .HasLink("/api/doc/jplag");
            });
        }

        public void RegisterPolicies(IAuthorizationPolicyContainer container)
        {
            container.AddPolicy2("HasDashboard", b => b.AcceptRole("PlagUser"));
        }
    }
}
