using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class ReminderChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskKey",
                table: "Reminders");

            migrationBuilder.AlterColumn<int[]>(
                name: "ResponsePacks",
                table: "Users",
                nullable: true,
                defaultValue: new[] { 0 },
                oldClrType: typeof(int[]),
                oldNullable: true,
                oldDefaultValue: new[] { 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int[]>(
                name: "ResponsePacks",
                table: "Users",
                nullable: true,
                defaultValue: new[] { 0 },
                oldClrType: typeof(int[]),
                oldNullable: true,
                oldDefaultValue: new[] { 0 });

            migrationBuilder.AddColumn<string>(
                name: "TaskKey",
                table: "Reminders",
                nullable: false,
                defaultValue: "");
        }
    }
}
