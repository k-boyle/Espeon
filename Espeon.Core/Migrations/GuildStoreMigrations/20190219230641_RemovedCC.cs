using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Core.Migrations.GuildStoreMigrations
{
    public partial class RemovedCC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomCommands_Guilds_GuildId",
                table: "CustomCommands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomCommands",
                table: "CustomCommands");

            migrationBuilder.RenameTable(
                name: "CustomCommands",
                newName: "CustomCommand");

            migrationBuilder.RenameIndex(
                name: "IX_CustomCommands_GuildId",
                table: "CustomCommand",
                newName: "IX_CustomCommand_GuildId");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1550617601334L,
                oldClrType: typeof(long),
                oldDefaultValue: 1550533481396L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomCommand",
                table: "CustomCommand",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomCommand_Guilds_GuildId",
                table: "CustomCommand",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomCommand_Guilds_GuildId",
                table: "CustomCommand");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomCommand",
                table: "CustomCommand");

            migrationBuilder.RenameTable(
                name: "CustomCommand",
                newName: "CustomCommands");

            migrationBuilder.RenameIndex(
                name: "IX_CustomCommand_GuildId",
                table: "CustomCommands",
                newName: "IX_CustomCommands_GuildId");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1550533481396L,
                oldClrType: typeof(long),
                oldDefaultValue: 1550617601334L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomCommands",
                table: "CustomCommands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomCommands_Guilds_GuildId",
                table: "CustomCommands",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
