using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class AddProofFilePathToExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Just add the new column to the existing Expenses table
            migrationBuilder.AddColumn<string>(
                name: "ProofFilePath",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Remove the column if migration is rolled back
            migrationBuilder.DropColumn(
                name: "ProofFilePath",
                table: "Expenses");
        }
    }
}
