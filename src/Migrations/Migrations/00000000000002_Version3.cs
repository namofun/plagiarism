using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SatelliteSite.Migrations
{
    [DbContext(typeof(ProductionContext))]
    [Migration("00000000000002_Version3")]
    public partial class Version3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ReportPending",
                table: "PlagiarismSets",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: migrationBuilder.IsNpgsql() ? "integer" : "int");

            migrationBuilder.AlterColumn<long>(
                name: "ReportCount",
                table: "PlagiarismSets",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: migrationBuilder.IsNpgsql() ? "integer" : "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReportPending",
                table: "PlagiarismSets",
                type: migrationBuilder.IsNpgsql() ? "integer" : "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "ReportCount",
                table: "PlagiarismSets",
                type: migrationBuilder.IsNpgsql() ? "integer" : "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
