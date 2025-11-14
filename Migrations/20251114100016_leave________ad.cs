using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class leave________ad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Employees_ApprovedBy",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_ApprovedBy",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Leaves",
                newName: "OverallStatus");

            migrationBuilder.RenameColumn(
                name: "LeaveType",
                table: "Leaves",
                newName: "ManagerStatus");

            migrationBuilder.RenameColumn(
                name: "ApproverComment",
                table: "Leaves",
                newName: "ManagerRemark");

            migrationBuilder.AlterColumn<double>(
                name: "TotalDays",
                table: "Leaves",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Leaves",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Leaves",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ContactDuringLeave",
                table: "Leaves",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressDuringLeave",
                table: "Leaves",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Leaves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DirectorRemark",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DirectorStatus",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HalfDaySession",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HrRemark",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HrStatus",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeValue",
                table: "Leaves",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "DirectorRemark",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "DirectorStatus",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "HalfDaySession",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "HrRemark",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "HrStatus",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "TimeValue",
                table: "Leaves");

            migrationBuilder.RenameColumn(
                name: "OverallStatus",
                table: "Leaves",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ManagerStatus",
                table: "Leaves",
                newName: "LeaveType");

            migrationBuilder.RenameColumn(
                name: "ManagerRemark",
                table: "Leaves",
                newName: "ApproverComment");

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "Leaves",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactDuringLeave",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressDuringLeave",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "Leaves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ApprovedBy",
                table: "Leaves",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Employees_ApprovedBy",
                table: "Leaves",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
