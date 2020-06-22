﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SatelliteSite.Data;

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(PlagiarismContext))]
    partial class PlagiarismContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SatelliteSite.Data.CheckSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReportCount")
                        .HasColumnType("int");

                    b.Property<int>("ReportPending")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("PlagiarismSets","plag");
                });

            modelBuilder.Entity("SatelliteSite.Data.Compilation", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Tokens")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.ToTable("PlagiarismCompilations","plag");
                });

            modelBuilder.Entity("SatelliteSite.Data.MatchReport", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWSEQUENTIALID()");

                    b.Property<int>("BiggestMatch")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<byte[]>("Matches")
                        .HasColumnType("varbinary(max)");

                    b.Property<bool>("Pending")
                        .HasColumnType("bit");

                    b.Property<double>("Percent")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0.0);

                    b.Property<double>("PercentA")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0.0);

                    b.Property<double>("PercentB")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0.0);

                    b.Property<int>("SubmissionA")
                        .HasColumnType("int");

                    b.Property<int>("SubmissionB")
                        .HasColumnType("int");

                    b.Property<int>("TokensMatched")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.HasIndex("SubmissionA");

                    b.HasIndex("SubmissionB");

                    b.ToTable("PlagiarismReports","plag");
                });

            modelBuilder.Entity("SatelliteSite.Data.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Language")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("MaxPercent")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SetId")
                        .HasColumnType("int");

                    b.Property<bool?>("TokenProduced")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("UploadTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("SetId");

                    b.ToTable("PlagiarismSubmissions","plag");
                });

            modelBuilder.Entity("SatelliteSite.Data.SubmissionFile", b =>
                {
                    b.Property<int>("SubmissionId")
                        .HasColumnType("int");

                    b.Property<int>("FileId")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SubmissionId", "FileId");

                    b.ToTable("PlagiarismFiles","plag");
                });

            modelBuilder.Entity("SatelliteSite.Data.Compilation", b =>
                {
                    b.HasOne("SatelliteSite.Data.Submission", null)
                        .WithOne()
                        .HasForeignKey("SatelliteSite.Data.Compilation", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SatelliteSite.Data.MatchReport", b =>
                {
                    b.HasOne("SatelliteSite.Data.Submission", null)
                        .WithMany()
                        .HasForeignKey("SubmissionA")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("SatelliteSite.Data.Submission", null)
                        .WithMany()
                        .HasForeignKey("SubmissionB")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("SatelliteSite.Data.Submission", b =>
                {
                    b.HasOne("SatelliteSite.Data.CheckSet", null)
                        .WithMany("Submissions")
                        .HasForeignKey("SetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SatelliteSite.Data.SubmissionFile", b =>
                {
                    b.HasOne("SatelliteSite.Data.Submission", null)
                        .WithMany("Files")
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
