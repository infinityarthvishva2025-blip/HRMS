using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class AddLateMarkDeductionToPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PayrollRecords",
                table: "PayrollRecords");

            migrationBuilder.RenameTable(
                name: "PayrollRecords",
                newName: "Payrolls");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payrolls",
                table: "Payrolls",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Payrolls",
                table: "Payrolls");

            migrationBuilder.RenameTable(
                name: "Payrolls",
                newName: "PayrollRecords");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayrollRecords",
                table: "PayrollRecords",
                column: "Id");
        }
    }
}
