using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class addstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AlternateNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "TwelfthPercentage",
                table: "Employees",
                newName: "PostGraduationCourse");

            migrationBuilder.RenameColumn(
                name: "JioTag",
                table: "Employees",
                newName: "LastCompanyName");

            migrationBuilder.RenameColumn(
                name: "IFSCCode",
                table: "Employees",
                newName: "GraduationCourse");

            migrationBuilder.RenameColumn(
                name: "GraduationPercentage",
                table: "Employees",
                newName: "ExperienceType");

            migrationBuilder.RenameColumn(
                name: "BranchName",
                table: "Employees",
                newName: "AlternateMobileNumber");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<int>(
                name: "GraduationPercent",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HSCPercent",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PostGraduationPercent",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalExperienceYears",
                table: "Employees",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GraduationPercent",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HSCPercent",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PostGraduationPercent",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TotalExperienceYears",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "PostGraduationCourse",
                table: "Employees",
                newName: "TwelfthPercentage");

            migrationBuilder.RenameColumn(
                name: "LastCompanyName",
                table: "Employees",
                newName: "JioTag");

            migrationBuilder.RenameColumn(
                name: "GraduationCourse",
                table: "Employees",
                newName: "IFSCCode");

            migrationBuilder.RenameColumn(
                name: "ExperienceType",
                table: "Employees",
                newName: "GraduationPercentage");

            migrationBuilder.RenameColumn(
                name: "AlternateMobileNumber",
                table: "Employees",
                newName: "BranchName");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Employees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlternateNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
