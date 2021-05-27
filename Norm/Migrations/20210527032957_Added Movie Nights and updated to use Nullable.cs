using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class AddedMovieNightsandupdatedtouseNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "timezone_id",
                table: "all_user_time_zones",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                table: "all_guild_prefixes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "event_name",
                table: "all_guild_events",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "event_desc",
                table: "all_guild_events",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "job_name",
                table: "all_guild_background_jobs",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "all_guild_movie_nights",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    voting_start_hangfire_id = table.Column<string>(type: "text", nullable: false),
                    voting_end_hangfire_id = table.Column<string>(type: "text", nullable: false),
                    movie_night_start_hangfire_id = table.Column<string>(type: "text", nullable: false),
                    number_of_suggestions = table.Column<int>(type: "integer", nullable: false),
                    maximum_rating = table.Column<int>(type: "integer", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    announcement_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    host_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    voting_message_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    selected_movie_index = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("movie_night_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "all_guild_movie_suggestions",
                columns: table => new
                {
                    imdb_id = table.Column<string>(type: "text", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    suggester_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_all_guild_movie_suggestions", x => new { x.imdb_id, x.guild_id });
                });

            migrationBuilder.CreateTable(
                name: "movie_night_and_suggestion_join",
                columns: table => new
                {
                    MovieNightId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MovieSuggestionId = table.Column<string>(type: "text", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    emoji_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movie_night_and_suggestion_join", x => new { x.MovieNightId, x.MovieSuggestionId });
                    table.ForeignKey(
                        name: "FK_movie_night_and_suggestion_join_all_guild_movie_nights_Movi~",
                        column: x => x.MovieNightId,
                        principalTable: "all_guild_movie_nights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movie_night_and_suggestion_join_all_guild_movie_suggestions~",
                        columns: x => new { x.MovieSuggestionId, x.GuildId },
                        principalTable: "all_guild_movie_suggestions",
                        principalColumns: new[] { "imdb_id", "guild_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_all_user_time_zones_user_id",
                table: "all_user_time_zones",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_all_guild_prefixes_guild_id",
                table: "all_guild_prefixes",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_all_guild_moderation_audit_records_guild_id",
                table: "all_guild_moderation_audit_records",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_all_guild_log_channels_guild_id",
                table: "all_guild_log_channels",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_all_guild_events_guild_id",
                table: "all_guild_events",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_all_guild_background_jobs_guild_id_scheduled_time",
                table: "all_guild_background_jobs",
                columns: new[] { "guild_id", "scheduled_time" });

            migrationBuilder.CreateIndex(
                name: "IX_all_guild_movie_nights_guild_id",
                table: "all_guild_movie_nights",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_movie_night_and_suggestion_join_MovieSuggestionId_GuildId",
                table: "movie_night_and_suggestion_join",
                columns: new[] { "MovieSuggestionId", "GuildId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movie_night_and_suggestion_join");

            migrationBuilder.DropTable(
                name: "all_guild_movie_nights");

            migrationBuilder.DropTable(
                name: "all_guild_movie_suggestions");

            migrationBuilder.DropIndex(
                name: "IX_all_user_time_zones_user_id",
                table: "all_user_time_zones");

            migrationBuilder.DropIndex(
                name: "IX_all_guild_prefixes_guild_id",
                table: "all_guild_prefixes");

            migrationBuilder.DropIndex(
                name: "IX_all_guild_moderation_audit_records_guild_id",
                table: "all_guild_moderation_audit_records");

            migrationBuilder.DropIndex(
                name: "IX_all_guild_log_channels_guild_id",
                table: "all_guild_log_channels");

            migrationBuilder.DropIndex(
                name: "IX_all_guild_events_guild_id",
                table: "all_guild_events");

            migrationBuilder.DropIndex(
                name: "IX_all_guild_background_jobs_guild_id_scheduled_time",
                table: "all_guild_background_jobs");

            migrationBuilder.AlterColumn<string>(
                name: "timezone_id",
                table: "all_user_time_zones",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                table: "all_guild_prefixes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "event_name",
                table: "all_guild_events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "event_desc",
                table: "all_guild_events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "job_name",
                table: "all_guild_background_jobs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
