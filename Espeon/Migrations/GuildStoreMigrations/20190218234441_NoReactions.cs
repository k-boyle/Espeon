using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations.GuildStoreMigrations
{
    public partial class NoReactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1550533481396L,
                oldClrType: typeof(long),
                oldDefaultValue: 1550453192723L);

            migrationBuilder.AddColumn<decimal>(
                name: "NoReactions",
                table: "Guilds",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoReactions",
                table: "Guilds");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1550453192723L,
                oldClrType: typeof(long),
                oldDefaultValue: 1550533481396L);
        }
    }
}
