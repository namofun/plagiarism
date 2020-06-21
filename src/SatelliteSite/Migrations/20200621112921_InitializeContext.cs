using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class InitializeContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "plag");

            migrationBuilder.CreateTable(
                name: "PlagiarismSets",
                schema: "plag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateTime = table.Column<DateTimeOffset>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ReportCount = table.Column<int>(nullable: false),
                    ReportPending = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismSubmissions",
                schema: "plag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    MaxPercent = table.Column<double>(nullable: false),
                    TokenProduced = table.Column<bool>(nullable: true),
                    UploadTime = table.Column<DateTimeOffset>(nullable: false),
                    Language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlagiarismSubmissions_PlagiarismSets_SetId",
                        column: x => x.SetId,
                        principalSchema: "plag",
                        principalTable: "PlagiarismSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismFiles",
                schema: "plag",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(nullable: false),
                    FileId = table.Column<int>(nullable: false),
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
                        principalSchema: "plag",
                        principalTable: "PlagiarismSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismReports",
                schema: "plag",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SubmissionA = table.Column<int>(nullable: false),
                    SubmissionB = table.Column<int>(nullable: false),
                    TokensMatched = table.Column<int>(nullable: false, defaultValue: 0),
                    BiggestMatch = table.Column<int>(nullable: false, defaultValue: 0),
                    Percent = table.Column<double>(nullable: false, defaultValue: 0.0),
                    PercentA = table.Column<double>(nullable: false, defaultValue: 0.0),
                    PercentB = table.Column<double>(nullable: false, defaultValue: 0.0),
                    Pending = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlagiarismReports_PlagiarismSubmissions_SubmissionA",
                        column: x => x.SubmissionA,
                        principalSchema: "plag",
                        principalTable: "PlagiarismSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlagiarismReports_PlagiarismSubmissions_SubmissionB",
                        column: x => x.SubmissionB,
                        principalSchema: "plag",
                        principalTable: "PlagiarismSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismTokens",
                schema: "plag",
                columns: table => new
                {
                    TokenId = table.Column<int>(nullable: false),
                    SubmissionId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Line = table.Column<int>(nullable: false),
                    FileId = table.Column<int>(nullable: false),
                    Column = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismTokens", x => new { x.SubmissionId, x.TokenId });
                    table.ForeignKey(
                        name: "FK_PlagiarismTokens_PlagiarismSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "plag",
                        principalTable: "PlagiarismSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlagiarismMatches",
                schema: "plag",
                columns: table => new
                {
                    MatchingId = table.Column<int>(nullable: false),
                    ReportId = table.Column<Guid>(nullable: false),
                    FileA = table.Column<int>(nullable: false),
                    FileB = table.Column<int>(nullable: false),
                    ContentStartA = table.Column<int>(nullable: false),
                    ContentEndA = table.Column<int>(nullable: false),
                    ContentStartB = table.Column<int>(nullable: false),
                    ContentEndB = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismMatches", x => new { x.ReportId, x.MatchingId });
                    table.ForeignKey(
                        name: "FK_PlagiarismMatches_PlagiarismReports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "plag",
                        principalTable: "PlagiarismReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlagiarismReports_SubmissionA",
                schema: "plag",
                table: "PlagiarismReports",
                column: "SubmissionA");

            migrationBuilder.CreateIndex(
                name: "IX_PlagiarismReports_SubmissionB",
                schema: "plag",
                table: "PlagiarismReports",
                column: "SubmissionB");

            migrationBuilder.CreateIndex(
                name: "IX_PlagiarismSubmissions_SetId",
                schema: "plag",
                table: "PlagiarismSubmissions",
                column: "SetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlagiarismFiles",
                schema: "plag");

            migrationBuilder.DropTable(
                name: "PlagiarismMatches",
                schema: "plag");

            migrationBuilder.DropTable(
                name: "PlagiarismTokens",
                schema: "plag");

            migrationBuilder.DropTable(
                name: "PlagiarismReports",
                schema: "plag");

            migrationBuilder.DropTable(
                name: "PlagiarismSubmissions",
                schema: "plag");

            migrationBuilder.DropTable(
                name: "PlagiarismSets",
                schema: "plag");
        }
    }
}
