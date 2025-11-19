using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class _10th : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GraduationMarksheetFilePath",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostGraduationMarksheetFilePath",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenthMarksheetFilePath",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwelfthMarksheetFilePath",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GraduationMarksheetFilePath",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PostGraduationMarksheetFilePath",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TenthMarksheetFilePath",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TwelfthMarksheetFilePath",
                table: "Employees");
        }
    }
}
