using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class ExpensesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Employees",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
            //        MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MotherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DOB_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ExperienceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TotalExperienceYears = table.Column<int>(type: "int", nullable: true),
            //        LastCompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        ReportingManager = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        HSCPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        GraduationCourse = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        GraduationPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        PostGraduationCourse = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PostGraduationPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        AadhaarNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PanNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AccountHolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IFSC = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Employees", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "GeoTags",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TagId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Latitude = table.Column<double>(type: "float", nullable: false),
            //        Longitude = table.Column<double>(type: "float", nullable: false),
            //        RadiusMeters = table.Column<int>(type: "int", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_GeoTags", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Hrs",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        HrId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Hrs", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "LeaveApprovalRoutes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CurrentRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        NextRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LevelOrder = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_LeaveApprovalRoutes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Payrolls",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Designation = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PAN = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        BankAccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            //        BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DateOfJoining = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        MonthYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TotalWorkingDays = table.Column<int>(type: "int", nullable: false),
            //        DaysAttended = table.Column<int>(type: "int", nullable: false),
            //        TotalLeavesTaken = table.Column<int>(type: "int", nullable: false),
            //        DeductionForLeaves = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        DeductionForLateMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        PerformanceAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        ProfessionalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TotalEarning = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Payrolls", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Attendances",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeId = table.Column<int>(type: "int", nullable: false),
            //        CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        CheckoutStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsLate = table.Column<bool>(type: "bit", nullable: false),
            //        IsEarlyLeave = table.Column<bool>(type: "bit", nullable: false),
            //        WorkingHours = table.Column<double>(type: "float", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Attendances", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Attendances_Employees_EmployeeId",
            //            column: x => x.EmployeeId,
            //            principalTable: "Employees",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProofFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HRComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            //migrationBuilder.CreateTable(
            //    name: "Leaves",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeId = table.Column<int>(type: "int", nullable: false),
            //        Category = table.Column<int>(type: "int", nullable: false),
            //        StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        HalfDaySession = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TimeValue = table.Column<TimeSpan>(type: "time", nullable: true),
            //        Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ContactDuringLeave = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AddressDuringLeave = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TotalDays = table.Column<double>(type: "float", nullable: false),
            //        ManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        HrStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DirectorStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        OverallStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        ManagerRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DirectorRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        HrRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CurrentApproverRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        NextApproverRole = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Leaves", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Leaves_Employees_EmployeeId",
            //            column: x => x.EmployeeId,
            //            principalTable: "Employees",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Attendances_EmployeeId",
            //    table: "Attendances",
            //    column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_EmployeeId",
                table: "Expenses",
                column: "EmployeeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Leaves_EmployeeId",
            //    table: "Leaves",
            //    column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "Attendances");

            migrationBuilder.DropTable(
                name: "Expenses");

            //migrationBuilder.DropTable(
            //    name: "GeoTags");

            //migrationBuilder.DropTable(
            //    name: "Hrs");

            //migrationBuilder.DropTable(
            //    name: "LeaveApprovalRoutes");

            //migrationBuilder.DropTable(
            //    name: "Leaves");

            //migrationBuilder.DropTable(
            //    name: "Payrolls");

            //migrationBuilder.DropTable(
            //    name: "Employees");
        }
    }
}
