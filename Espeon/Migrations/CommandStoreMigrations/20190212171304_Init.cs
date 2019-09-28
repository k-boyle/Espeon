using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

namespace Espeon.Migrations.CommandStoreMigrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Aliases = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "CommandInfo",
                columns: table => new
                {
                    ModuleName = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Aliases = table.Column<List<string>>(nullable: true),
                    Responses = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandInfo", x => new { x.Name, x.ModuleName });
                    table.ForeignKey(
                        name: "FK_CommandInfo_Modules_ModuleName",
                        column: x => x.ModuleName,
                        principalTable: "Modules",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandInfo_ModuleName",
                table: "CommandInfo",
                column: "ModuleName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandInfo");

            migrationBuilder.DropTable(
                name: "Modules");
        }
    }
}
