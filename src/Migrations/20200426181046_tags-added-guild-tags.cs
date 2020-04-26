using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class tagsaddedguildtags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "tags",
                newName: "GuildId");

            migrationBuilder.CreateTable(
                name: "guild_tags",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_tags", x => x.guild_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tags_GuildId",
                table: "tags",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_guild_tags_guild_id",
                table: "guild_tags",
                column: "guild_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tags_guild_tags_GuildId",
                table: "tags",
                column: "GuildId",
                principalTable: "guild_tags",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tags_guild_tags_GuildId",
                table: "tags");

            migrationBuilder.DropTable(
                name: "guild_tags");

            migrationBuilder.DropIndex(
                name: "IX_tags_GuildId",
                table: "tags");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "tags",
                newName: "guild_id");
        }
    }
}
