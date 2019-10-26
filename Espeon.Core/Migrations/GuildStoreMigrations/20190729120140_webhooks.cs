using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Core.Migrations.GuildStoreMigrations
{
    public partial class webhooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1564401700628L,
                oldClrType: typeof(long),
                oldDefaultValue: 1557334208982L);

            migrationBuilder.CreateTable(
                name: "MockWebhook",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    ChannelId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockWebhook", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockWebhook_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockWebhook_GuildId",
                table: "MockWebhook",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockWebhook");

            migrationBuilder.AlterColumn<long>(
                name: "IssuedOn",
                table: "Warning",
                nullable: false,
                defaultValue: 1557334208982L,
                oldClrType: typeof(long),
                oldDefaultValue: 1564401700628L);
        }
    }
}
