using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

namespace Espeon.Migrations.GuildStoreMigrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    WelcomeChannelId = table.Column<decimal>(nullable: false),
                    WelcomeMessage = table.Column<string>(nullable: true),
                    DefaultRoleId = table.Column<decimal>(nullable: false),
                    Prefixes = table.Column<List<string>>(nullable: true),
                    RestrictedChannels = table.Column<string>(nullable: true),
                    RestrictedUsers = table.Column<string>(nullable: true),
                    Admins = table.Column<string>(nullable: true),
                    Moderators = table.Column<string>(nullable: true),
                    SelfAssigningRoles = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomCommands",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomCommands_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomCommands_GuildId",
                table: "CustomCommands",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomCommands");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
