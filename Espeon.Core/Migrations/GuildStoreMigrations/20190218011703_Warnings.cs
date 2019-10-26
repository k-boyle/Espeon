using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Core.Migrations.GuildStoreMigrations
{
    public partial class Warnings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarningLimit",
                table: "Guilds",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.CreateTable(
                name: "Warning",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    TargetUser = table.Column<decimal>(nullable: false),
                    Issuer = table.Column<decimal>(nullable: false),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warning_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warning_GuildId",
                table: "Warning",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Warning");

            migrationBuilder.DropColumn(
                name: "WarningLimit",
                table: "Guilds");
        }
    }
}
