using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend.Jobs;
using Plag.Backend.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Plag.Backend.Tests
{
    public class Database : DbContext
    {
        public Database(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new PlagEntityConfiguration<Database>().Configure(modelBuilder, this);
        }
    }

    public sealed class ServiceProviderFixture : IServiceProvider, IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public void Dispose() => ServiceProvider?.Dispose();

        public object GetService(Type serviceType)
        {
            return ((IServiceProvider)ServiceProvider).GetService(serviceType);
        }

        public ServiceProviderFixture()
        {
            var services = new ServiceCollection();
            services.AddDbContext<Database>(b => b.UseInMemoryDatabase("PlagDevTests", b => b.UseBulk()), optionsLifetime: ServiceLifetime.Singleton);
            services.AddSingleton(typeof(SequentialGuidGenerator<>));
            services.AddSingleton<ICompileService, NullCompileService>();
            services.AddSingleton(typeof(IResettableSignal<>), typeof(NullResettableSignal<>));
            services.AddScoped<IPlagiarismDetectService, EntityFrameworkCoreStoreService<Database>>();
            ServiceProvider = services.BuildServiceProvider(true);
        }
    }

    public class DefaultConventions : IClassFixture<ServiceProviderFixture>
    {
        private IServiceProvider ServiceProvider { get; }

        public DefaultConventions(ServiceProviderFixture serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        [Fact]
        public async Task NotExists_ReturnsNull()
        {
            using var scope = ServiceProvider.CreateScope();
            var pds = scope.ServiceProvider.GetRequiredService<IPlagiarismDetectService>();
            var nullGuid = Guid.Empty.ToString();

            Assert.Null(await pds.FindSubmissionAsync(nullGuid, 0, true));
            Assert.Null(await pds.FindSetAsync(nullGuid));
            Assert.Null(await pds.FindReportAsync(nullGuid));
            Assert.Null(await pds.FindLanguageAsync("cpp"));
            Assert.Null(await pds.GetComparisonsBySubmissionAsync(nullGuid, 0));
            Assert.Null(await pds.GetCompilationAsync(nullGuid, 0));
            Assert.Null(await pds.GetFilesAsync(nullGuid, 0));
        }

        [Fact]
        public async Task NotExists_ThrowsException()
        {
            using var scope = ServiceProvider.CreateScope();
            var pds = scope.ServiceProvider.GetRequiredService<IPlagiarismDetectService>();
            var nullGuid = Guid.Empty.ToString();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => pds.SubmitAsync(new Models.SubmissionCreation { SetId = nullGuid }));
        }
    }
}
