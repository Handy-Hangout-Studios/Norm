using Microsoft.EntityFrameworkCore.Migrations;

namespace Norm.Migrations
{
    public partial class AddDMRoyalRoadAnnouncements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_dm",
                table: "guild_novel_registration",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "member_id",
                table: "guild_novel_registration",
                type: "numeric(20,0)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_dm",
                table: "guild_novel_registration");

            migrationBuilder.DropColumn(
                name: "member_id",
                table: "guild_novel_registration");
        }
    }
}
