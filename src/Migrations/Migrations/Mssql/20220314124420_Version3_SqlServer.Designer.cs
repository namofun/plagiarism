﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SatelliteSite;

#nullable disable

namespace SatelliteSite.Migrations.Mssql
{
    [DbContext(typeof(MssqlDesignTimeContext))]
    [Migration("20220314124420_Version3_SqlServer")]
    partial class Version3_SqlServer
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.PlagiarismSet<System.Guid>", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("ContestId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("ReportCount")
                        .HasColumnType("bigint");

                    b.Property<long>("ReportPending")
                        .HasColumnType("bigint");

                    b.Property<int>("SubmissionCount")
                        .HasColumnType("int");

                    b.Property<int>("SubmissionFailed")
                        .HasColumnType("int");

                    b.Property<int>("SubmissionSucceeded")
                        .HasColumnType("int");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ContestId");

                    b.HasIndex("UserId");

                    b.ToTable("PlagiarismSets", (string)null);
                });

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.Report<System.Guid>", b =>
                {
                    b.Property<Guid>("SetId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("SubmissionA")
                        .HasColumnType("int");

                    b.Property<int>("SubmissionB")
                        .HasColumnType("int");

                    b.Property<int>("BiggestMatch")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<Guid>("ExternalId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool?>("Finished")
                        .HasColumnType("bit");

                    b.Property<bool?>("Justification")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Matches")
                        .HasColumnType("varbinary(max)");

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

                    b.Property<string>("SessionKey")
                        .HasMaxLength(25)
                        .IsUnicode(false)
                        .HasColumnType("varchar(25)");

                    b.Property<bool>("Shared")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<int>("TokensMatched")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("SetId", "SubmissionA", "SubmissionB");

                    b.HasAlternateKey("ExternalId");

                    b.HasIndex("SetId", "SubmissionB");

                    b.ToTable("PlagiarismReports", (string)null);
                });

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.Submission<System.Guid>", b =>
                {
                    b.Property<Guid>("SetId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ExclusiveCategory")
                        .HasColumnType("int");

                    b.Property<Guid>("ExternalId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("InclusiveCategory")
                        .HasColumnType("int");

                    b.Property<string>("Language")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("MaxPercent")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("TokenProduced")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Tokens")
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTimeOffset>("UploadTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("SetId", "Id");

                    b.ToTable("PlagiarismSubmissions", (string)null);
                });

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.SubmissionFile<System.Guid>", b =>
                {
                    b.Property<Guid>("SubmissionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("FileId")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SubmissionId", "FileId");

                    b.ToTable("PlagiarismFiles", (string)null);
                });

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.Report<System.Guid>", b =>
                {
                    b.HasOne("Xylab.PlagiarismDetect.Backend.Entities.PlagiarismSet<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("SetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Xylab.PlagiarismDetect.Backend.Entities.Submission<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("SetId", "SubmissionA")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Xylab.PlagiarismDetect.Backend.Entities.Submission<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("SetId", "SubmissionB")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.Submission<System.Guid>", b =>
                {
                    b.HasOne("Xylab.PlagiarismDetect.Backend.Entities.PlagiarismSet<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("SetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Xylab.PlagiarismDetect.Backend.Entities.SubmissionFile<System.Guid>", b =>
                {
                    b.HasOne("Xylab.PlagiarismDetect.Backend.Entities.Submission<System.Guid>", null)
                        .WithMany()
                        .HasForeignKey("SubmissionId")
                        .HasPrincipalKey("ExternalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
