using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class tagsaddedguildtags2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tags_guild_tags_GuildId",
                table: "tags");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "tags",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_tags_GuildId",
                table: "tags",
                newName: "IX_tags_guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tags_guild_tags_guild_id",
                table: "tags",
                column: "guild_id",
                principalTable: "guild_tags",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tags_guild_tags_guild_id",
                table: "tags");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "tags",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_tags_guild_id",
                table: "tags",
                newName: "IX_tags_GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_tags_guild_tags_GuildId",
                table: "tags",
                column: "GuildId",
                principalTable: "guild_tags",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
