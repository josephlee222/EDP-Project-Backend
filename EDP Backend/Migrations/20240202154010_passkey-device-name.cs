using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDP_Backend.Migrations
{
    /// <inheritdoc />
    public partial class passkeydevicename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "Credentials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "Credentials");
        }
    }
}
