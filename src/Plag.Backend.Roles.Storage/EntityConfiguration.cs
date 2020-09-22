using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Plag.Backend.Entities;
using System;

namespace Plag.Backend
{
    public class PlagEntityConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Submission>,
        IEntityTypeConfiguration<Compilation>,
        IEntityTypeConfiguration<SubmissionFile>,
        IEntityTypeConfiguration<PlagiarismSet>,
        IEntityTypeConfiguration<Report>
        where TContext : DbContext
    {
        private readonly ValueConverter<string, Guid> _converter;

        public PlagEntityConfiguration()
        {
            _converter = new ValueConverter<string, Guid>(
                s => new Guid(s),
                g => g.ToString(),
                new ConverterMappingHints(valueGeneratorFactory: (_, __) => new SequentialGuidValueGenerator()));
        }

        public void Configure(EntityTypeBuilder<Submission> entity)
        {
            entity.ToTable("PlagiarismSubmissions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasConversion(_converter);

            entity.Property(e => e.SetId)
                .HasConversion(_converter);

            entity.HasOne<PlagiarismSet>()
                .WithMany(s => s.Submissions)
                .HasForeignKey(e => e.SetId);
        }

        public void Configure(EntityTypeBuilder<Compilation> entity)
        {
            entity.ToTable("PlagiarismCompilations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasConversion(_converter);

            entity.HasOne<Submission>()
                .WithOne()
                .HasForeignKey<Compilation>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<SubmissionFile> entity)
        {
            entity.ToTable("PlagiarismFiles");

            entity.HasKey(e => new { e.SubmissionId, e.FileId });

            entity.Property(e => e.SubmissionId)
                .HasConversion(_converter);

            entity.HasOne<Submission>()
                .WithMany(e => e.Files)
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public void Configure(EntityTypeBuilder<PlagiarismSet> entity)
        {
            entity.ToTable("PlagiarismSets");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasConversion(_converter);
        }

        public void Configure(EntityTypeBuilder<Report> entity)
        {
            entity.ToTable("PlagiarismReports");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasConversion(_converter)
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.SubmissionA)
                .HasConversion(_converter);

            entity.Property(e => e.SubmissionB)
                .HasConversion(_converter);

            entity.Property(e => e.BiggestMatch).HasDefaultValue(0);
            entity.Property(e => e.Percent).HasDefaultValue(0.0);
            entity.Property(e => e.PercentA).HasDefaultValue(0.0);
            entity.Property(e => e.PercentB).HasDefaultValue(0.0);
            entity.Property(e => e.TokensMatched).HasDefaultValue(0);

            entity.HasIndex(e => e.SubmissionA);
            entity.HasIndex(e => e.SubmissionB);

            entity.HasOne<Submission>()
                .WithMany()
                .HasForeignKey(e => e.SubmissionA)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Submission>()
                .WithMany()
                .HasForeignKey(e => e.SubmissionB)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
