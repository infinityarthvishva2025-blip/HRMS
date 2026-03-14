using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class attendace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "DaysAttended",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "LateMarksOver3",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "NetPay",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "OtherAllowances",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "PerformanceAllowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "ProfessionalTax",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "TotalLeavesTaken",
                table: "Payroll");

            migrationBuilder.RenameColumn(
                name: "Reimbursement",
                table: "Payroll",
                newName: "reimbursement");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "Payroll",
                newName: "month");

            migrationBuilder.RenameColumn(
                name: "PetrolAllowance",
                table: "Payroll",
                newName: "petrol_allowance");

            migrationBuilder.RenameColumn(
                name: "PaidDays",
                table: "Payroll",
                newName: "day_presented");

            migrationBuilder.RenameColumn(
                name: "LateMarks",
                table: "Payroll",
                newName: "late_marks");

            migrationBuilder.RenameColumn(
                name: "LateDeductionDays",
                table: "Payroll",
                newName: "late_deduction_days");

            migrationBuilder.RenameColumn(
                name: "EmployeeCode",
                table: "Payroll",
                newName: "emp_code");

            migrationBuilder.RenameColumn(
                name: "BaseSalary",
                table: "Payroll",
                newName: "base_salary");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "Payroll",
                newName: "working_days");

            migrationBuilder.RenameColumn(
                name: "TotalWorkingDays",
                table: "Payroll",
                newName: "late_gt_3");

            migrationBuilder.RenameColumn(
                name: "TotalEarning",
                table: "Payroll",
                newName: "leaves_taken");

            migrationBuilder.RenameColumn(
                name: "EmployeeName",
                table: "Payroll",
                newName: "name");

            migrationBuilder.AlterColumn<decimal>(
                name: "reimbursement",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "petrol_allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "employee_ctc",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "gross_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "other_allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "paid_days",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "per_day_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "perf_allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "prof_tax",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_deduction",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_pay",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Attendances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "Att_Date",
                table: "Attendances",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "employee_ctc",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "gross_salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "net_salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "other_allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "paid_days",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "per_day_salary",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "perf_allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "prof_tax",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "total_deduction",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "total_pay",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "Att_Date",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "reimbursement",
                table: "Payroll",
                newName: "Reimbursement");

            migrationBuilder.RenameColumn(
                name: "month",
                table: "Payroll",
                newName: "Month");

            migrationBuilder.RenameColumn(
                name: "petrol_allowance",
                table: "Payroll",
                newName: "PetrolAllowance");

            migrationBuilder.RenameColumn(
                name: "late_marks",
                table: "Payroll",
                newName: "LateMarks");

            migrationBuilder.RenameColumn(
                name: "late_deduction_days",
                table: "Payroll",
                newName: "LateDeductionDays");

            migrationBuilder.RenameColumn(
                name: "emp_code",
                table: "Payroll",
                newName: "EmployeeCode");

            migrationBuilder.RenameColumn(
                name: "day_presented",
                table: "Payroll",
                newName: "PaidDays");

            migrationBuilder.RenameColumn(
                name: "base_salary",
                table: "Payroll",
                newName: "BaseSalary");

            migrationBuilder.RenameColumn(
                name: "working_days",
                table: "Payroll",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Payroll",
                newName: "EmployeeName");

            migrationBuilder.RenameColumn(
                name: "leaves_taken",
                table: "Payroll",
                newName: "TotalEarning");

            migrationBuilder.RenameColumn(
                name: "late_gt_3",
                table: "Payroll",
                newName: "TotalWorkingDays");

            migrationBuilder.AlterColumn<decimal>(
                name: "Reimbursement",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PetrolAllowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Payroll",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DaysAttended",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LateMarksOver3",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NetPay",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherAllowances",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PerformanceAllowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfessionalTax",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalLeavesTaken",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
