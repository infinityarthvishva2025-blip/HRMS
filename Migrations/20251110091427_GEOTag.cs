using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class GEOTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "InTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "JioTag",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "OutTime",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Attendances",
                newName: "FaceVerificationResponse");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Attendances",
                newName: "TimestampUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Leaves",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HalfDayType",
                table: "Leaves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaveDate",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeId",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "FaceVerified",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GeoTagId",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Attendances",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Attendances",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "GeoTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    RadiusMeters = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoTags", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GeoTags",
                columns: new[] { "Id", "Description", "Latitude", "Longitude", "RadiusMeters", "TagId" },
                values: new object[] { 1, "Mumbai Office", 19.076000000000001, 72.877700000000004, 100000, "Office-001" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_GeoTagId",
                table: "Attendances",
                column: "GeoTagId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoTags_TagId",
                table: "GeoTags",
                column: "TagId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_GeoTags_GeoTagId",
                table: "Attendances",
                column: "GeoTagId",
                principalTable: "GeoTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_GeoTags_GeoTagId",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "GeoTags");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_GeoTagId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "HalfDayType",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "LeaveDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ToDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "FaceVerified",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "GeoTagId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "TimestampUtc",
                table: "Attendances",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "FaceVerificationResponse",
                table: "Attendances",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Leaves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "InTime",
                table: "Attendances",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JioTag",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OutTime",
                table: "Attendances",
                type: "time",
                nullable: true);
        }
    }
}
