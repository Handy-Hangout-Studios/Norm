using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class AddGuildWelcomeMessageSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "all_guild_welcome_message_settings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    should_welcome_members = table.Column<bool>(type: "boolean", nullable: false),
                    should_ping = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("guild_id", x => x.GuildId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "all_guild_welcome_message_settings");
        }
    }
}
