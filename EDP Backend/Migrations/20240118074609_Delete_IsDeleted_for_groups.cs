using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDP_Backend.Migrations
{
    /// <inheritdoc />
    public partial class Delete_IsDeleted_for_groups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Groups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
