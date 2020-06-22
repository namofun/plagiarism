using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class CompilationStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlagiarismCompilations",
                schema: "plag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    Tokens = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlagiarismCompilations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlagiarismCompilations_PlagiarismSubmissions_Id",
                        column: x => x.Id,
                        principalSchema: "plag",
                        principalTable: "PlagiarismSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlagiarismCompilations",
                schema: "plag");
        }
    }
}
