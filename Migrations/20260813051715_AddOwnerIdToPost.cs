using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogmanager_phamvanbinhminh.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Posts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Posts");
        }
    }
}
