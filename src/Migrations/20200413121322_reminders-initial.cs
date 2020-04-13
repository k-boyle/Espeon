using Microsoft.EntityFrameworkCore.Migrations;

namespace Espeon.Migrations
{
    public partial class remindersinitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    channel_id = table.Column<decimal>(nullable: false),
                    user_id = table.Column<decimal>(nullable: false),
                    message_id = table.Column<decimal>(nullable: false),
                    trigger_at = table.Column<long>(nullable: false),
                    reminder_string = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reminders");
        }
    }
}
