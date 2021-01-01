using Microsoft.EntityFrameworkCore.Migrations;

namespace Harold.Migrations
{
    public partial class AddForeignKeyAndConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ping_everyone",
                table: "GuildNovelRegistrations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ping_no_one",
                table: "GuildNovelRegistrations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GuildNovelRegistrations_novel_info_id",
                table: "GuildNovelRegistrations",
                column: "novel_info_id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildNovelRegistrations_AllNovelInfo_novel_info_id",
                table: "GuildNovelRegistrations",
                column: "novel_info_id",
                principalTable: "AllNovelInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildNovelRegistrations_AllNovelInfo_novel_info_id",
                table: "GuildNovelRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_GuildNovelRegistrations_novel_info_id",
                table: "GuildNovelRegistrations");

            migrationBuilder.DropColumn(
                name: "ping_everyone",
                table: "GuildNovelRegistrations");

            migrationBuilder.DropColumn(
                name: "ping_no_one",
                table: "GuildNovelRegistrations");
        }
    }
}
