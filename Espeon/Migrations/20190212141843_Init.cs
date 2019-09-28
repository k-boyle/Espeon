using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    ResponsePack = table.Column<int>(nullable: false, defaultValue: 0),
                    CandyAmount = table.Column<int>(nullable: false, defaultValue: 10),
                    HighestCandies = table.Column<int>(nullable: false, defaultValue: 10),
                    LastClaimedCandies = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TheReminder = table.Column<string>(nullable: true),
                    JumpUrl = table.Column<string>(nullable: true),
                    UserId = table.Column<decimal>(nullable: false),
                    GuildId = table.Column<decimal>(nullable: false),
                    ChannelId = table.Column<decimal>(nullable: false),
                    ReminderId = table.Column<int>(nullable: false),
                    TaskKey = table.Column<string>(nullable: true),
                    WhenToRemove = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
