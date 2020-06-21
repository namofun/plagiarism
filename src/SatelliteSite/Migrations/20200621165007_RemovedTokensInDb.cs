using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    public partial class RemovedTokensInDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlagiarismTokens",
                schema: "plag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlagiarismTokens",
                schema: "plag",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    TokenId = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    Line = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
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
        }
    }
}
