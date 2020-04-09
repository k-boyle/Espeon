using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prefixes",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(nullable: false),
                    prefixes = table.Column<string[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prefixes", x => x.guild_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prefixes_guild_id",
                table: "prefixes",
                column: "guild_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prefixes");
        }
    }
}
