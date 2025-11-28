using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    public partial class AddAnnouncementsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Title = table.Column<string>(nullable: false),
                    Message = table.Column<string>(nullable: false),

                    TargetDepartment = table.Column<string>(nullable: true),
                    TargetEmployeeId = table.Column<int>(nullable: true),

                    IsUrgent = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),

                    EmployeeId = table.Column<int>(nullable: false),
                    IsRead = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");
        }
    }
}
