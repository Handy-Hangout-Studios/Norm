using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class UseImdbIdinsteadofselectedindexforstoringwinningsuggestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "selected_movie_index",
                table: "all_guild_movie_nights",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "selected_movie_index",
                table: "all_guild_movie_nights",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
