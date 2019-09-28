using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations.GuildStoreMigrations
{
    public partial class WarningsIssuedOn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1550453192723L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedOn",
                table: "Warning");
        }
    }
}
