using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class AddyearfieldtoMovieSuggestiontable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "all_guild_movie_suggestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "year",
                table: "all_guild_movie_suggestions");
        }
    }
}
