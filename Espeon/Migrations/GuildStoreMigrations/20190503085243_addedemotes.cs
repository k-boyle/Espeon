using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations.GuildStoreMigrations
{
    public partial class addedemotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1556873563472L,
                oldClrType: typeof(long),
                oldDefaultValue: 1551953334643L);

            migrationBuilder.AddColumn<bool>(
                name: "EmotesEnabled",
                table: "Guilds",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmotesEnabled",
                table: "Guilds");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1551953334643L,
                oldClrType: typeof(long),
                oldDefaultValue: 1556873563472L);
        }
    }
}
