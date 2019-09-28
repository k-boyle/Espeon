using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class guid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int[]>(
                name: "ResponsePacks",
                table: "Users",
                nullable: true,
                defaultValue: new[] { 0 },
                oldClrType: typeof(int[]),
                oldNullable: true,
                oldDefaultValue: new[] { 0 });

            migrationBuilder.AlterColumn<string>(
                name: "TaskKey",
                table: "Reminders",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
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

            migrationBuilder.AlterColumn<string>(
                name: "TaskKey",
                table: "Reminders",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
