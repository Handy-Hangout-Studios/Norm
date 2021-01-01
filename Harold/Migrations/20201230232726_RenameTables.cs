using Microsoft.EntityFrameworkCore.Migrations;

namespace Harold.Migrations
{
    public partial class RenameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildNovelRegistrations_AllNovelInfo_novel_info_id",
                table: "GuildNovelRegistrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildNovelRegistrations",
                table: "GuildNovelRegistrations");

            migrationBuilder.RenameTable(
                name: "GuildNovelRegistrations",
                newName: "guild_novel_registration");

            migrationBuilder.RenameTable(
                name: "AllNovelInfo",
                newName: "novel_info");

            migrationBuilder.RenameIndex(
                name: "IX_GuildNovelRegistrations_novel_info_id",
                table: "guild_novel_registration",
                newName: "IX_guild_novel_registration_novel_info_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_novel_registration",
                table: "guild_novel_registration",
                columns: new[] { "guild_id", "novel_info_id", "announcement_channel_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_guild_novel_registration_novel_info_novel_info_id",
                table: "guild_novel_registration",
                column: "novel_info_id",
                principalTable: "novel_info",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_novel_registration_novel_info_novel_info_id",
                table: "guild_novel_registration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_novel_registration",
                table: "guild_novel_registration");

            migrationBuilder.RenameTable(
                name: "novel_info",
                newName: "AllNovelInfo");

            migrationBuilder.RenameTable(
                name: "guild_novel_registration",
                newName: "GuildNovelRegistrations");

            migrationBuilder.RenameIndex(
                name: "IX_guild_novel_registration_novel_info_id",
                table: "GuildNovelRegistrations",
                newName: "IX_GuildNovelRegistrations_novel_info_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildNovelRegistrations",
                table: "GuildNovelRegistrations",
                columns: new[] { "guild_id", "novel_info_id", "announcement_channel_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_GuildNovelRegistrations_AllNovelInfo_novel_info_id",
                table: "GuildNovelRegistrations",
                column: "novel_info_id",
                principalTable: "AllNovelInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
