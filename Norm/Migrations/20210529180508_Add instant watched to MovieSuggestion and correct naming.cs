using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace Norm.Migrations
{
    public partial class AddinstantwatchedtoMovieSuggestionandcorrectnaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movie_night_and_suggestion_join_all_guild_movie_nights_Movi~",
                table: "movie_night_and_suggestion_join");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "movie_night_and_suggestion_join",
                newName: "guild_id");

            migrationBuilder.RenameColumn(
                name: "MovieSuggestionId",
                table: "movie_night_and_suggestion_join",
                newName: "movie_suggestion_id");

            migrationBuilder.RenameColumn(
                name: "MovieNightId",
                table: "movie_night_and_suggestion_join",
                newName: "movie_night_id");

            migrationBuilder.RenameIndex(
                name: "IX_movie_night_and_suggestion_join_MovieSuggestionId_GuildId",
                table: "movie_night_and_suggestion_join",
                newName: "IX_movie_night_and_suggestion_join_movie_suggestion_id_guild_id");

            migrationBuilder.AddColumn<Instant>(
                name: "instant_watched",
                table: "all_guild_movie_suggestions",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_movie_night_and_suggestion_join_all_guild_movie_nights_movi~",
                table: "movie_night_and_suggestion_join",
                column: "movie_night_id",
                principalTable: "all_guild_movie_nights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_movie_night_and_suggestion_join_all_guild_movie_nights_movi~",
                table: "movie_night_and_suggestion_join");

            migrationBuilder.DropColumn(
                name: "instant_watched",
                table: "all_guild_movie_suggestions");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "movie_night_and_suggestion_join",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "movie_suggestion_id",
                table: "movie_night_and_suggestion_join",
                newName: "MovieSuggestionId");

            migrationBuilder.RenameColumn(
                name: "movie_night_id",
                table: "movie_night_and_suggestion_join",
                newName: "MovieNightId");

            migrationBuilder.RenameIndex(
                name: "IX_movie_night_and_suggestion_join_movie_suggestion_id_guild_id",
                table: "movie_night_and_suggestion_join",
                newName: "IX_movie_night_and_suggestion_join_MovieSuggestionId_GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_movie_night_and_suggestion_join_all_guild_movie_nights_Movi~",
                table: "movie_night_and_suggestion_join",
                column: "MovieNightId",
                principalTable: "all_guild_movie_nights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
