using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class CreateAssetsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductHardware = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            //migrationBuilder.CreateTable(
            //    name: "Employees",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        MotherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DOB_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        JioTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AlternateNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BloodGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Employees", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Expenses",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ExpenseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ProofFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Date = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Expenses", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "GeoTags",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TagId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
            //    name: "Leaves",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        LeaveType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        HalfDayType = table.Column<int>(type: "int", nullable: false),
            //        Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
            //        EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Leaves", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Payrolls",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        HRA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        TA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Bonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Deductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
            //        EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        GeoTagId = table.Column<int>(type: "int", nullable: false),
            //        TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Latitude = table.Column<double>(type: "float", nullable: false),
            //        Longitude = table.Column<double>(type: "float", nullable: false),
            //        FaceVerified = table.Column<bool>(type: "bit", nullable: false),
            //        FaceVerificationResponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Attendances", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Attendances_GeoTags_GeoTagId",
            //            column: x => x.GeoTagId,
            //            principalTable: "GeoTags",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.InsertData(
            //    table: "GeoTags",
            //    columns: new[] { "Id", "Description", "Latitude", "Longitude", "RadiusMeters", "TagId" },
            //    values: new object[] { 1, "Mumbai Office", 19.076000000000001, 72.877700000000004, 100000, "Office-001" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Attendances_GeoTagId",
            //    table: "Attendances",
            //    column: "GeoTagId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_GeoTags_TagId",
            //    table: "GeoTags",
            //    column: "TagId",
            //    unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            //migrationBuilder.DropTable(
            //    name: "Attendances");

            //migrationBuilder.DropTable(
            //    name: "Employees");

            //migrationBuilder.DropTable(
            //    name: "Expenses");

            //migrationBuilder.DropTable(
            //    name: "Leaves");

            //migrationBuilder.DropTable(
            //    name: "Payrolls");

            //migrationBuilder.DropTable(
            //    name: "GeoTags");
        }
    }
}
