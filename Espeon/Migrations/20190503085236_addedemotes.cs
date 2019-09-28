using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class addedemotes : Migration
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

            migrationBuilder.AddColumn<bool>(
                name: "BoughtEmotes",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoughtEmotes",
                table: "Users");

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
