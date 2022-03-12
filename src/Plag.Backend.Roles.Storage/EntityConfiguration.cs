using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Xylab.PlagiarismDetect.Backend.Entities;

namespace Xylab.PlagiarismDetect.Backend
{
    public class PlagEntityConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Submission<Guid>>,
        IEntityTypeConfiguration<SubmissionFile<Guid>>,
        IEntityTypeConfiguration<PlagiarismSet<Guid>>,
        IEntityTypeConfiguration<Report<Guid>>
        where TContext : DbContext
    {
        public void Configure(EntityTypeBuilder<Submission<Guid>> entity)
        {
            entity.ToTable("PlagiarismSubmissions");

            entity.HasKey(e => new { e.SetId, e.Id });

            entity.HasAlternateKey(e => e.ExternalId);

            entity.HasOne<PlagiarismSet<Guid>>()
                .WithMany()
                .HasForeignKey(e => e.SetId);
        }

        public void Configure(EntityTypeBuilder<SubmissionFile<Guid>> entity)
        {
            entity.ToTable("PlagiarismFiles");

            entity.HasKey(e => new { e.SubmissionId, e.FileId });

            entity.HasOne<Submission<Guid>>()
                .WithMany()
                .HasForeignKey(e => e.SubmissionId)
                .HasPrincipalKey(e => e.ExternalId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<PlagiarismSet<Guid>> entity)
        {
            entity.ToTable("PlagiarismSets");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.UserId);

            entity.HasIndex(e => e.ContestId);
        }

        public void Configure(EntityTypeBuilder<Report<Guid>> entity)
        {
            entity.ToTable("PlagiarismReports");

            entity.HasKey(e => new { e.SetId, e.SubmissionA, e.SubmissionB });

            entity.HasAlternateKey(e => e.ExternalId);

            entity.Property(e => e.BiggestMatch).HasDefaultValue(0);
            entity.Property(e => e.Percent).HasDefaultValue(0.0);
            entity.Property(e => e.PercentA).HasDefaultValue(0.0);
            entity.Property(e => e.PercentB).HasDefaultValue(0.0);
            entity.Property(e => e.TokensMatched).HasDefaultValue(0);
            entity.Property(e => e.Shared).HasDefaultValue(false);

            entity.Property(e => e.SessionKey)
                .IsUnicode(false)
                .HasMaxLength(25);

            entity.HasOne<PlagiarismSet<Guid>>()
                .WithMany()
                .HasForeignKey(e => e.SetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Submission<Guid>>()
                .WithMany()
                .HasForeignKey(e => new { e.SetId, e.SubmissionA })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Submission<Guid>>()
                .WithMany()
                .HasForeignKey(e => new { e.SetId, e.SubmissionB })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
