using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class Update_all_dependencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "selected_movie_index",
                table: "all_guild_movie_nights",
                newName: "winning_movie_imdb_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "winning_movie_imdb_id",
                table: "all_guild_movie_nights",
                newName: "selected_movie_index");
        }
    }
}
