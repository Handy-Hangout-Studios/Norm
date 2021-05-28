using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class UseDiscordEmojinameforEmojiIdinsteadofulong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "emoji_id",
                table: "movie_night_and_suggestion_join",
                type: "text",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "emoji_id",
                table: "movie_night_and_suggestion_join",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
