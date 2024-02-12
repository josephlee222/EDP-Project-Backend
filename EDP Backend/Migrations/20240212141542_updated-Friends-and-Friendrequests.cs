using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDP_Backend.Migrations
{
    /// <inheritdoc />
    public partial class updatedFriendsandFriendrequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FriendID",
                table: "Friends",
                newName: "SenderID");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "FriendRequests",
                newName: "SentAt");

            migrationBuilder.AddColumn<int>(
                name: "RecipientID",
                table: "Friends",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientID",
                table: "Friends");

            migrationBuilder.RenameColumn(
                name: "SenderID",
                table: "Friends",
                newName: "FriendID");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "FriendRequests",
                newName: "CreatedAt");
        }
    }
}
