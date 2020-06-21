using Microsoft.EntityFrameworkCore;
using System;

namespace SatelliteSite.Data
{
    public class PlagiarismContext : DbContext
    {
        public virtual DbSet<Submission> Submissions { get; set; }
        public virtual DbSet<MatchReport> Reports { get; set; }
        public virtual DbSet<CheckSet> CheckSets { get; set; }

        public PlagiarismContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Submission>(submission =>
            {
                submission.HasKey(e => e.Id);

                submission.HasOne<CheckSet>()
                    .WithMany(s => s.Submissions)
                    .HasForeignKey(e => e.SetId);
            });

            modelBuilder.Entity<Token>(token =>
            {
                token.Property<int>("SubmissionId");

                token.HasOne<Submission>()
                    .WithMany(e => e.Tokens)
                    .HasForeignKey("SubmissionId")
                    .OnDelete(DeleteBehavior.Cascade);

                token.Property(e => e.TokenId)
                    .ValueGeneratedNever();

                token.HasKey("SubmissionId", "TokenId");
            });

            modelBuilder.Entity<SubmissionFile>(file =>
            {
                file.HasKey(e => new { e.SubmissionId, e.FileId });

                file.HasOne<Submission>()
                    .WithMany(e => e.Files)
                    .HasForeignKey(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MatchReport>(result =>
            {
                result.HasKey(e => e.Id);

                result.Property(e => e.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                result.Property(e => e.BiggestMatch).HasDefaultValue(0);
                result.Property(e => e.Percent).HasDefaultValue(0.0);
                result.Property(e => e.PercentA).HasDefaultValue(0.0);
                result.Property(e => e.PercentB).HasDefaultValue(0.0);
                result.Property(e => e.TokensMatched).HasDefaultValue(0);

                result.HasIndex(e => e.SubmissionA);
                result.HasIndex(e => e.SubmissionB);

                result.HasOne<Submission>()
                    .WithMany()
                    .HasForeignKey(e => e.SubmissionA)
                    .OnDelete(DeleteBehavior.Restrict);

                result.HasOne<Submission>()
                    .WithMany()
                    .HasForeignKey(e => e.SubmissionB)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MatchPair>(match =>
            {
                match.Property<Guid>("ReportId");

                match.HasOne<MatchReport>()
                    .WithMany(e => e.MatchPairs)
                    .HasForeignKey("ReportId")
                    .OnDelete(DeleteBehavior.Cascade);

                match.Property(e => e.MatchingId)
                    .ValueGeneratedNever();

                match.HasKey("ReportId", "MatchingId");
            });

            modelBuilder.Entity<CheckSet>(result =>
            {
                result.HasKey(e => e.Id);
            });
        }
    }
}
