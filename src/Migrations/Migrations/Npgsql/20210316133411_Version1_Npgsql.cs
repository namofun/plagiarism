using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations.Npgsql
{
    public partial class Version1_Npgsql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlagiarismSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: true),
                    ContestId = table.Column<int>(nullable: true),
                    CreateTime = table.Column<DateTimeOffset>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ReportCount = table.Column<int>(nullable: false),
                    ReportPending = table.Column<int>(nullable: false),
                    SubmissionCount = table.Column<int>(nullable: false),
                    SubmissionFailed = table.Column<int>(nullable: false),
                    SubmissionSucceeded = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismSubmissions",
                columns: table => new
                {
                    SetId = table.Column<Guid>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    ExternalId = table.Column<Guid>(nullable: false),
                    ExclusiveCategory = table.Column<int>(nullable: false),
                    InclusiveCategory = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    MaxPercent = table.Column<double>(nullable: false),
                    TokenProduced = table.Column<bool>(nullable: true),
                    UploadTime = table.Column<DateTimeOffset>(nullable: false),
                    Language = table.Column<string>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    Tokens = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismSubmissions", x => new { x.SetId, x.Id });
                    table.UniqueConstraint("AK_PlagiarismSubmissions_ExternalId", x => x.ExternalId);
                    table.ForeignKey(
                        name: "FK_PlagiarismSubmissions_PlagiarismSets_SetId",
                        column: x => x.SetId,
                        principalTable: "PlagiarismSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismFiles",
                columns: table => new
                {
                    FileId = table.Column<int>(nullable: false),
                    SubmissionId = table.Column<Guid>(nullable: false),
                    FilePath = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismFiles", x => new { x.SubmissionId, x.FileId });
                    table.ForeignKey(
                        name: "FK_PlagiarismFiles_PlagiarismSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "PlagiarismSubmissions",
                        principalColumn: "ExternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismReports",
                columns: table => new
                {
                    SetId = table.Column<Guid>(nullable: false),
                    SubmissionA = table.Column<int>(nullable: false),
                    SubmissionB = table.Column<int>(nullable: false),
                    ExternalId = table.Column<Guid>(nullable: false),
                    TokensMatched = table.Column<int>(nullable: false, defaultValue: 0),
                    BiggestMatch = table.Column<int>(nullable: false, defaultValue: 0),
                    Percent = table.Column<double>(nullable: false, defaultValue: 0.0),
                    PercentA = table.Column<double>(nullable: false, defaultValue: 0.0),
                    PercentB = table.Column<double>(nullable: false, defaultValue: 0.0),
                    Finished = table.Column<bool>(nullable: true),
                    Matches = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismReports", x => new { x.SetId, x.SubmissionA, x.SubmissionB });
                    table.UniqueConstraint("AK_PlagiarismReports_ExternalId", x => x.ExternalId);
                    table.ForeignKey(
                        name: "FK_PlagiarismReports_PlagiarismSets_SetId",
                        column: x => x.SetId,
                        principalTable: "PlagiarismSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlagiarismReports_PlagiarismSubmissions_SetId_SubmissionA",
                        columns: x => new { x.SetId, x.SubmissionA },
                        principalTable: "PlagiarismSubmissions",
                        principalColumns: new[] { "SetId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlagiarismReports_PlagiarismSubmissions_SetId_SubmissionB",
                        columns: x => new { x.SetId, x.SubmissionB },
                        principalTable: "PlagiarismSubmissions",
                        principalColumns: new[] { "SetId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlagiarismReports_SetId_SubmissionB",
                table: "PlagiarismReports",
                columns: new[] { "SetId", "SubmissionB" });

            migrationBuilder.CreateIndex(
                name: "IX_PlagiarismSets_ContestId",
                table: "PlagiarismSets",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlagiarismSets_UserId",
                table: "PlagiarismSets",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlagiarismFiles");

            migrationBuilder.DropTable(
                name: "PlagiarismReports");

            migrationBuilder.DropTable(
                name: "PlagiarismSubmissions");

            migrationBuilder.DropTable(
                name: "PlagiarismSets");
        }
    }
}
