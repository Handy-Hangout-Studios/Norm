using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Norm.Migrations
{
    public partial class InitialCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "all_guild_background_jobs",
                columns: table => new
                {
                    HangfireJobId = table.Column<string>(type: "text", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    job_name = table.Column<string>(type: "text", nullable: true),
                    scheduled_time = table.Column<Instant>(type: "timestamp", nullable: false),
                    job_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("hangfire_job_id", x => x.HangfireJobId);
                });

            migrationBuilder.CreateTable(
                name: "all_guild_events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    event_name = table.Column<string>(type: "text", nullable: true),
                    event_desc = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("guild_event_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "all_guild_log_channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    log_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("guild_log_channel_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "all_guild_moderation_audit_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    moderator_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    moderation_action = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_record_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "all_guild_prefixes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prefix = table.Column<string>(type: "text", nullable: true),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("guild_prefix_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "all_user_time_zones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    timezone_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_timezone_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "novel_info",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fiction_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    novel_name = table.Column<string>(type: "text", nullable: false),
                    syndication_uri = table.Column<string>(type: "text", nullable: false),
                    fiction_uri = table.Column<string>(type: "text", nullable: false),
                    most_recent_chapter_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("novel_info_id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guild_novel_registration",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    announcement_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    novel_info_id = table.Column<int>(type: "integer", nullable: false),
                    ping_everyone = table.Column<bool>(type: "boolean", nullable: false),
                    ping_no_one = table.Column<bool>(type: "boolean", nullable: false),
                    role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_novel_registration", x => new { x.guild_id, x.novel_info_id, x.announcement_channel_id });
                    table.ForeignKey(
                        name: "FK_guild_novel_registration_novel_info_novel_info_id",
                        column: x => x.novel_info_id,
                        principalTable: "novel_info",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_novel_registration_novel_info_id",
                table: "guild_novel_registration",
                column: "novel_info_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "all_guild_background_jobs");

            migrationBuilder.DropTable(
                name: "all_guild_events");

            migrationBuilder.DropTable(
                name: "all_guild_log_channels");

            migrationBuilder.DropTable(
                name: "all_guild_moderation_audit_records");

            migrationBuilder.DropTable(
                name: "all_guild_prefixes");

            migrationBuilder.DropTable(
                name: "all_user_time_zones");

            migrationBuilder.DropTable(
                name: "guild_novel_registration");

            migrationBuilder.DropTable(
                name: "novel_info");
        }
    }
}
