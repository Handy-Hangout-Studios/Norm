using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Harold.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllNovelInfo",
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
                    table.PrimaryKey("id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildNovelRegistrations",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    announcement_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    novel_info_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildNovelRegistrations", x => new { x.guild_id, x.novel_info_id, x.announcement_channel_id });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllNovelInfo");

            migrationBuilder.DropTable(
                name: "GuildNovelRegistrations");
        }
    }
}
