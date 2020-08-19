using Microsoft.EntityFrameworkCore;
using System;

namespace SatelliteSite.Data
{
    public class PlagiarismContext : DbContext
    {
        public virtual DbSet<PlagiarismSubmission> Submissions { get; set; }
        public virtual DbSet<PlagiarismReport> Reports { get; set; }
        public virtual DbSet<PlagiarismSet> CheckSets { get; set; }

        public PlagiarismContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
