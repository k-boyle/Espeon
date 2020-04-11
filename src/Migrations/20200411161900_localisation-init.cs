using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class localisationinit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "localisation",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(nullable: false),
                    user_id = table.Column<decimal>(nullable: false),
                    localisation = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localisation", x => new { x.guild_id, x.user_id });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "localisation");
        }
    }
}
