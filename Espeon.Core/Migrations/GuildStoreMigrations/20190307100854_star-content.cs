using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Core.Migrations.GuildStoreMigrations
{
    public partial class starcontent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1551953334643L,
                oldClrType: typeof(long),
                oldDefaultValue: 1551284553496L);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "StarredMessage",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "StarredMessage");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1551284553496L,
                oldClrType: typeof(long),
                oldDefaultValue: 1551953334643L);
        }
    }
}
