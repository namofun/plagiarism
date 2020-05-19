using Microsoft.EntityFrameworkCore;
using SatelliteSite.Data.Demos;

namespace SatelliteSite.Data
{
    public class DemoContext : DbContext
    {
        public virtual DbSet<Family> Families { get; set; }

        public DemoContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Family>(family =>
            {
                family.HasKey(e => e.Id);

                family.OwnsMany(e => e.Parents);

                family.OwnsMany(e => e.Children, child =>
                {
                    child.OwnsMany(e => e.Pets);
                });

                family.OwnsOne(e => e.Address);
            });
        }
    }
}
