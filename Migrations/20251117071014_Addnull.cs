using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class Addnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
        name: "CheckInTime",
        table: "Attendances",
        type: "datetime2",
        nullable: true,
        oldClrType: typeof(DateTime),
        oldType: "datetime2",
        oldNullable: false
    );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
        name: "CheckInTime",
        table: "Attendances",
        type: "datetime2",
        nullable: false,
        defaultValue: new DateTime(2000, 1, 1), // fallback if rollback needed
        oldClrType: typeof(DateTime),
        oldType: "datetime2",
        oldNullable: true
    );
        }
    }
}
