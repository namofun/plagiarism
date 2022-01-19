using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(ProductionContext))]
    [Migration("00000000000001_Version2")]
    public partial class Version2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string booleanType = migrationBuilder.IsSqlServer()
                ? "bit"
                : migrationBuilder.IsMySql()
                ? "tinyint(1)"
                : "boolean";

            string varChar25Type = migrationBuilder.IsSqlServer()
                ? "varchar(25)"
                : migrationBuilder.IsMySql()
                ? "varchar(25)"
                : "character varying(25)";

            migrationBuilder.AddColumn<bool>(
                name: "Justification",
                table: "PlagiarismReports",
                type: booleanType,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionKey",
                table: "PlagiarismReports",
                type: varChar25Type,
                unicode: false,
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Shared",
                table: "PlagiarismReports",
                type: booleanType,
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
