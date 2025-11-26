using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class PayrollRebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Base_Salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "DateOfJoining",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Day_Presented",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Days_Attended",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Emp_Code",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Gross_Salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Late_Deduction_Days",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Late_GT_3",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Late_Marks",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "MonthYear",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Net_Salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Other_Allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "PAN",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Paid_Days",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Per_Day_Salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Perf_Allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Petrol_Allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Prof_Tax",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Total_Deduction",
                table: "Payroll");

            migrationBuilder.RenameColumn(
                name: "Working_Days",
                table: "Payroll",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "Total_Pay",
                table: "Payroll",
                newName: "TotalDeductions");

            migrationBuilder.RenameColumn(
                name: "Leaves_Taken",
                table: "Payroll",
                newName: "DaysAttended");

            migrationBuilder.AlterColumn<int>(
                name: "Month",
                table: "Payroll",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Year",
                table: "Payroll",
                newName: "Working_Days");

            migrationBuilder.RenameColumn(
                name: "TotalDeductions",
                table: "Payroll",
                newName: "Total_Pay");

            migrationBuilder.RenameColumn(
                name: "DaysAttended",
                table: "Payroll",
                newName: "Leaves_Taken");

            migrationBuilder.AlterColumn<string>(
                name: "Month",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Base_Salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfJoining",
                table: "Payroll",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Day_Presented",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Days_Attended",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Emp_Code",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Gross_Salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Late_Deduction_Days",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Late_GT_3",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Late_Marks",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MonthYear",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Net_Salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Other_Allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PAN",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Paid_Days",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Per_Day_Salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Perf_Allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Petrol_Allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Prof_Tax",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total_Deduction",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
