using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class AddGurukulVideoPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "Payrolls");

            migrationBuilder.AlterColumn<string>(
                name: "TitleGroup",
                table: "GurukulVideos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "GurukulVideos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AllowedDepartment",
                table: "GurukulVideos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AllowedEmployeeId",
                table: "GurukulVideos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GurukulVideos_AllowedEmployeeId",
                table: "GurukulVideos",
                column: "AllowedEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GurukulVideos_Employees_AllowedEmployeeId",
                table: "GurukulVideos",
                column: "AllowedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GurukulVideos_Employees_AllowedEmployeeId",
                table: "GurukulVideos");

            migrationBuilder.DropIndex(
                name: "IX_GurukulVideos_AllowedEmployeeId",
                table: "GurukulVideos");

            migrationBuilder.DropColumn(
                name: "AllowedDepartment",
                table: "GurukulVideos");

            migrationBuilder.DropColumn(
                name: "AllowedEmployeeId",
                table: "GurukulVideos");

            migrationBuilder.AlterColumn<string>(
                name: "TitleGroup",
                table: "GurukulVideos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "GurukulVideos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            //migrationBuilder.CreateTable(
            //    name: "Payrolls",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        BankAccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            //        BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DateOfJoining = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DaysAttended = table.Column<int>(type: "int", nullable: false),
            //        DeductionForLateMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DeductionForLeaves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Designation = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        MonthYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        PAN = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        PerformanceAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        ProfessionalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TotalEarning = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TotalLeavesTaken = table.Column<int>(type: "int", nullable: false),
            //        TotalWorkingDays = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Payrolls", x => x.Id);
            //    });
        }
    }
}
