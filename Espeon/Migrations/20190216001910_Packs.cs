using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class Packs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "ResponsePacks",
                table: "Users",
                nullable: true,
                defaultValue: new[] { 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsePacks",
                table: "Users");
        }
    }
}
