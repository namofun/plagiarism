﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xylab.PlagiarismDetect.Backend.Jobs;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Backend
{
    public static class BackendGenerationExtensions
    {
        public static IServiceCollection AddPlagGenerationService(this IServiceCollection services)
        {
            services.TryAddSingleton<ICompileService, AntlrCompileService>();
            services.TryAddSingleton<IConvertService>(sp => sp.GetRequiredService<IConvertService2>());
            services.TryAddSingleton<IConvertService2, DefaultConvertService2>();
            services.TryAddSingleton<IReportService, GstReportService>();

            return services;
        }

        public static IServiceCollection AddPlagBackgroundService(this IServiceCollection services)
        {
            services.AddPlagGenerationService();

            services.AddHostedService<SubmissionTokenizeService>();
            services.AddHostedService<ReportGenerationService>();
            services.AddSingleton<ILanguageProvider, LocalLanguageProvider>();
            services.AddSingleton<ISignalProvider, BackgroundServiceSignalProvider>();

            return services;
        }

        public static IHostBuilder AddPlagBackgroundService(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddPlagBackgroundService();
            });
        }
    }
}
