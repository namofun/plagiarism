using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatelliteSite.Migrations.Npgsql
{
    public partial class Version2_Npgsql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Justification",
                table: "PlagiarismReports",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionKey",
                table: "PlagiarismReports",
                type: "character varying(25)",
                unicode: false,
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Shared",
                table: "PlagiarismReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Justification",
                table: "PlagiarismReports");

            migrationBuilder.DropColumn(
                name: "SessionKey",
                table: "PlagiarismReports");

            migrationBuilder.DropColumn(
                name: "Shared",
                table: "PlagiarismReports");
        }
    }
}
