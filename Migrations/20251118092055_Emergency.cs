using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class Emergency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiseaseName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiseaseSince",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorContact",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactAddress",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactMobile",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasDisease",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastAffectedDate",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalDocumentFilePath",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicinesRequired",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddress",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiseaseName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DiseaseSince",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DoctorContact",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactAddress",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactMobile",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HasDisease",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastAffectedDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MedicalDocumentFilePath",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MedicinesRequired",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermanentAddress",
                table: "Employees");
        }
    }
}
