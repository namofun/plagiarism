using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend
{
    public class RestfulBackendRole : IBackendRoleStrategy
    {
        public void Apply(IServiceCollection services)
        {
            services.AddScoped<IPlagiarismDetectService, RestfulStoreService>();

            services.AddOptions<PlagRestfulOptions>()
                .PostConfigure<IConfiguration>((options, configuration) =>
                {
                    if (string.IsNullOrEmpty(options.ServerAddress))
                    {
                        options.ServerAddress = configuration.GetConnectionString("PlagiarismBackendServer");
                    }

                    if (options.JsonSerializerOptions == null)
                    {
                        throw new ArgumentNullException("PlagRestfulOptions.JsonSerializerOptions");
                    }

                    if (string.IsNullOrWhiteSpace(options.ServerAddress) ||
                        !Uri.TryCreate(options.ServerAddress, UriKind.Absolute, out var baseUri))
                    {
                        throw new ArgumentException(
                            "Please fill in PlagRestfulOptions.ServerAddress or env:ConnectionStrings__PlagiarismBackendServer " +
                            "with URLs like 'http://pds-BEPROD'");
                    }
                });

            services.AddHttpClient<RestfulClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
                .ConfigureHttpClient((services, httpClient) =>
                {
                    var options = services.GetRequiredService<IOptions<PlagRestfulOptions>>();
                    var baseUri = new Uri(options.Value.ServerAddress, UriKind.Absolute);
                    httpClient.BaseAddress = baseUri;
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "PlagiarismRestful/1.3.0");
                });
        }
    }
}
