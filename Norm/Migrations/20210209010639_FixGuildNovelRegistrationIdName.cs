using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class FixGuildNovelRegistrationIdName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_novel_registration",
                table: "guild_novel_registration");

            migrationBuilder.AddPrimaryKey(
                name: "guild_novel_registration_id",
                table: "guild_novel_registration",
                columns: new[] { "guild_id", "novel_info_id", "announcement_channel_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "guild_novel_registration_id",
                table: "guild_novel_registration");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_novel_registration",
                table: "guild_novel_registration",
                columns: new[] { "guild_id", "novel_info_id", "announcement_channel_id" });
        }
    }
}
