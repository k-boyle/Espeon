using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Core.Migrations.GuildStoreMigrations
{
    public partial class Starboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1551284553496L,
                oldClrType: typeof(long),
                oldDefaultValue: 1550617601334L);

            migrationBuilder.AddColumn<int>(
                name: "StarLimit",
                table: "Guilds",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "StarboardChannelId",
                table: "Guilds",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "StarredMessage",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    AuthorId = table.Column<decimal>(nullable: false),
                    StarboardMessageId = table.Column<decimal>(nullable: false),
                    ReactionUsers = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarredMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarredMessage_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarredMessage_GuildId",
                table: "StarredMessage",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarredMessage");

            migrationBuilder.DropColumn(
                name: "StarLimit",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "StarboardChannelId",
                table: "Guilds");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1550617601334L,
                oldClrType: typeof(long),
                oldDefaultValue: 1551284553496L);
        }
    }
}
