using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Services;
using System;
using System.Net.Http;

namespace Plag.Backend
{
    public class RestfulBackendRole : IBackendRoleStrategy
    {
        public void Apply(IServiceCollection services)
        {
            services.AddHttpClient<IStoreService, RestfulStoreService>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
                .ConfigureHttpClient((services, httpClient) =>
                {
                    var configuration = services.GetRequiredService<IConfiguration>();
                    var baseUrl = configuration.GetConnectionString("PlagiarismBackendServer");

                    if (string.IsNullOrWhiteSpace(baseUrl) ||
                        !Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                    {
                        throw new ArgumentException(
                            "Please fill in the ConnectionStrings__PlagiarismBackendServer " +
                            "with URLs like 'http://pds-BEPROD'");
                    }

                    httpClient.BaseAddress = baseUri;
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "PlagiarismRestful/1.1.0");
                });
        }
    }
}
