using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCompOffLedgerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "day_presented",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "employee_ctc",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "late_deduction_days",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "late_gt_3",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "other_allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "perf_allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "petrol_allowance",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "prof_tax",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "reimbursement",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "TargetEmployeeId",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "TargetDepartment",
                table: "Announcements",
                newName: "TargetEmployees");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Announcements",
                newName: "IsGlobal");

            migrationBuilder.AlterColumn<int>(
                name: "working_days",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "total_deduction",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "per_day_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "paid_days",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "net_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "month",
                table: "Payroll",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "gross_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "base_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "Payroll",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompOff",
                table: "Leaves",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkDate",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AlternateMobileNumber",
                table: "Employees",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<double>(
                name: "CompOffBalance",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DeactiveReason",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompOffEarnedDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckInLatitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckInLongitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckOutLatitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckOutLongitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectionProofPath",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectionRemark",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CorrectionRequested",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CorrectionRequestedOn",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectionStatus",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompOffCredited",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGeoAttendance",
                table: "Attendances",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedOn",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReadByEmployees",
                table: "Announcements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetDepartments",
                table: "Announcements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompOffLedgers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedLeaveId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompOffLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompOffLedgers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    TodaysWork = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PendingWork = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Issues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_DailyReports_Employees_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HolidayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LockedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResignationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuggestedLastWorkingDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedLastWorkingDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReasonCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailedReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResignationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportRecipients_DailyReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "DailyReports",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReportRecipients_Employees_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResignationApprovalSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResignationRequestId = table.Column<int>(type: "int", nullable: false),
                    StepNo = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApproverEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ActionOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResignationApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResignationApprovalSteps_ResignationRequests_ResignationRequestId",
                        column: x => x.ResignationRequestId,
                        principalTable: "ResignationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompOffLedgers_EmployeeId",
                table: "CompOffLedgers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportRecipients_ReceiverId",
                table: "DailyReportRecipients",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportRecipients_ReportId",
                table: "DailyReportRecipients",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_SenderId",
                table: "DailyReports",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationApprovalSteps_ResignationRequestId",
                table: "ResignationApprovalSteps",
                column: "ResignationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_EmployeeId",
                table: "ResignationRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompOffLedgers");

            migrationBuilder.DropTable(
                name: "DailyReportRecipients");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "PayrollLocks");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "ResignationApprovalSteps");

            migrationBuilder.DropTable(
                name: "DailyReports");

            migrationBuilder.DropTable(
                name: "ResignationRequests");

            migrationBuilder.DropColumn(
                name: "year",
                table: "Payroll");

            migrationBuilder.DropColumn(
                name: "IsCompOff",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "CompOffBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DeactiveReason",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastCompOffEarnedDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CheckInLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CorrectionProofPath",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CorrectionRemark",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CorrectionRequested",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CorrectionRequestedOn",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CorrectionStatus",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsCompOffCredited",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsGeoAttendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ReviewedOn",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ReadByEmployees",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "TargetDepartments",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "TargetEmployees",
                table: "Announcements",
                newName: "TargetDepartment");

            migrationBuilder.RenameColumn(
                name: "IsGlobal",
                table: "Announcements",
                newName: "IsRead");

            migrationBuilder.AlterColumn<int>(
                name: "working_days",
                table: "Payroll",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_deduction",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "per_day_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "paid_days",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "net_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "month",
                table: "Payroll",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "gross_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "base_salary",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "day_presented",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "employee_ctc",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "late_deduction_days",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "late_gt_3",
                table: "Payroll",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "other_allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "perf_allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "petrol_allowance",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "prof_tax",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "reimbursement",
                table: "Payroll",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AlternateMobileNumber",
                table: "Employees",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetEmployeeId",
                table: "Announcements",
                type: "int",
                nullable: true);
        }
    }
}
