using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SatelliteSite
{
    internal class MssqlDesignTimeContext : DbContext
    {
        public MssqlDesignTimeContext(
            DbContextOptions<MssqlDesignTimeContext> options)
            : base(options)
        {
        }
    }

    internal class NpgsqlDesignTimeContext : DbContext
    {
        public NpgsqlDesignTimeContext(
            DbContextOptions<NpgsqlDesignTimeContext> options)
            : base(options)
        {
        }
    }

    internal class MysqlDesignTimeContext : DbContext
    {
        public MysqlDesignTimeContext(
            DbContextOptions<MysqlDesignTimeContext> options)
            : base(options)
        {
        }
    }

    internal class ProductionModelCustomizer<T> : IModelCustomizer where T : DbContext
    {
        public void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            new Plag.Backend.PlagEntityConfiguration<T>().Configure(modelBuilder);
        }
    }

    internal class DesignTimeFactoryMssql : IDesignTimeDbContextFactory<MssqlDesignTimeContext>
    {
        public MssqlDesignTimeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MssqlDesignTimeContext>();
            optionsBuilder.UseSqlServer("Host=localhost");
            optionsBuilder.ReplaceService<IModelCustomizer, ProductionModelCustomizer<MssqlDesignTimeContext>>();
            return new MssqlDesignTimeContext(optionsBuilder.Options);
        }
    }

    internal class DesignTimeFactoryNpgsql : IDesignTimeDbContextFactory<NpgsqlDesignTimeContext>
    {
        public NpgsqlDesignTimeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NpgsqlDesignTimeContext>();
            optionsBuilder.UseNpgsql("Host=localhost");
            optionsBuilder.ReplaceService<IModelCustomizer, ProductionModelCustomizer<NpgsqlDesignTimeContext>>();
            return new NpgsqlDesignTimeContext(optionsBuilder.Options);
        }
    }

    internal class DesignTimeFactoryMysql : IDesignTimeDbContextFactory<MysqlDesignTimeContext>
    {
        public MysqlDesignTimeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MysqlDesignTimeContext>();
            optionsBuilder.UseMySql("Host=localhost");
            optionsBuilder.ReplaceService<IModelCustomizer, ProductionModelCustomizer<MysqlDesignTimeContext>>();
            return new MysqlDesignTimeContext(optionsBuilder.Options);
        }
    }
}
