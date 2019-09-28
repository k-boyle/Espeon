using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations.GuildStoreMigrations
{
    public partial class autoquotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1557334208982L,
                oldClrType: typeof(long),
                oldDefaultValue: 1556873563472L);

            migrationBuilder.AddColumn<bool>(
                name: "AutoQuotes",
                table: "Guilds",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoQuotes",
                table: "Guilds");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1556873563472L,
                oldClrType: typeof(long),
                oldDefaultValue: 1557334208982L);
        }
    }
}
