using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class RemovedMatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlagiarismMatches",
                schema: "plag");

            migrationBuilder.AddColumn<byte[]>(
                name: "Matches",
                schema: "plag",
                table: "PlagiarismReports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Matches",
                schema: "plag",
                table: "PlagiarismReports");

            migrationBuilder.CreateTable(
                name: "PlagiarismMatches",
                schema: "plag",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchingId = table.Column<int>(type: "int", nullable: false),
                    ContentEndA = table.Column<int>(type: "int", nullable: false),
                    ContentEndB = table.Column<int>(type: "int", nullable: false),
                    ContentStartA = table.Column<int>(type: "int", nullable: false),
                    ContentStartB = table.Column<int>(type: "int", nullable: false),
                    FileA = table.Column<int>(type: "int", nullable: false),
                    FileB = table.Column<int>(type: "int", nullable: false)
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
        }
    }
}
