using Microsoft.EntityFrameworkCore;
using SatelliteSite.Data.Demos;
using SatelliteSite.Data.Match;
using SatelliteSite.Data.Submit;

namespace SatelliteSite.Data
{
    public class DemoContext : DbContext
    {
        public virtual DbSet<Family> Families { get; set; }
        public virtual DbSet<Submission> Submissions { get; set; }
        public virtual DbSet<Report> Results { get; set; }

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

            modelBuilder.Entity<Submission>(Submission =>
            {
                Submission.HasKey(e => e.Id);

                Submission.OwnsMany(e => e.Tokens);

                Submission.OwnsOne(e => e.Files);
            });

            modelBuilder.Entity<Report>(result =>
            {
                result.HasKey(e => e.Id);

                result.OwnsMany(e => e.MatchPairs);
            });
        }
    }
}
