using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class tagsinitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    tag_key = table.Column<string>(nullable: false),
                    tag_string = table.Column<string>(nullable: false),
                    created_at = table.Column<long>(nullable: false),
                    uses = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    guild_id = table.Column<decimal>(nullable: true),
                    created_id = table.Column<decimal>(nullable: true),
                    owner_id = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tags");
        }
    }
}
