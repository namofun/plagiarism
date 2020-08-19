using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SatelliteSite.Entities
{
    public class PlagEntityConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<PlagiarismSubmission>,
        IEntityTypeConfiguration<PlagiarismCompilation>,
        IEntityTypeConfiguration<PlagiarismFile>,
        IEntityTypeConfiguration<PlagiarismSet>,
        IEntityTypeConfiguration<PlagiarismReport>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<PlagiarismSubmission> entity)
        {
            entity.ToTable("PlagiarismSubmissions");

            entity.HasKey(e => e.Id);

            entity.HasOne<PlagiarismSet>()
                .WithMany(s => s.Submissions)
                .HasForeignKey(e => e.SetId);

            entity.Ignore(e => e.Tokens);
        }

        public void Configure(EntityTypeBuilder<PlagiarismCompilation> entity)
        {
            entity.ToTable("PlagiarismCompilations");

            entity.HasKey(e => e.Id);

            entity.HasOne<PlagiarismSubmission>()
                .WithOne()
                .HasForeignKey<PlagiarismCompilation>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<PlagiarismFile> entity)
        {
            entity.ToTable("PlagiarismFiles");

            entity.HasKey(e => new { e.SubmissionId, e.FileId });

            entity.HasOne<PlagiarismSubmission>()
                .WithMany(e => e.Files)
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<PlagiarismSet> entity)
        {
            entity.ToTable("PlagiarismSets");

            entity.HasKey(e => e.Id);
        }

        public void Configure(EntityTypeBuilder<PlagiarismReport> entity)
        {
            entity.ToTable("PlagiarismReports");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.BiggestMatch).HasDefaultValue(0);
            entity.Property(e => e.Percent).HasDefaultValue(0.0);
            entity.Property(e => e.PercentA).HasDefaultValue(0.0);
            entity.Property(e => e.PercentB).HasDefaultValue(0.0);
            entity.Property(e => e.TokensMatched).HasDefaultValue(0);

            entity.HasIndex(e => e.SubmissionA);
            entity.HasIndex(e => e.SubmissionB);

            entity.HasOne<PlagiarismSubmission>()
                .WithMany()
                .HasForeignKey(e => e.SubmissionA)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<PlagiarismSubmission>()
                .WithMany()
                .HasForeignKey(e => e.SubmissionB)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
