using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Core.Migrations.CommandStoreMigrations
{
    public partial class removedrespones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Responses",
                table: "CommandInfo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Responses",
                table: "CommandInfo",
                nullable: true);
        }
    }
}
