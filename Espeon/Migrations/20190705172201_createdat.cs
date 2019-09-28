using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class createdat : Migration
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

            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "Reminders",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
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
    }
}
