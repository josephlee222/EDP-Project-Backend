using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDP_Backend.Migrations
{
    /// <inheritdoc />
    public partial class availabilityrefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Carts_AvailabilityId",
                table: "Carts",
                column: "AvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_ActivityId",
                table: "Availabilities",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availabilities_Activities_ActivityId",
                table: "Availabilities",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Availabilities_AvailabilityId",
                table: "Carts",
                column: "AvailabilityId",
                principalTable: "Availabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availabilities_Activities_ActivityId",
                table: "Availabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Availabilities_AvailabilityId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_AvailabilityId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Availabilities_ActivityId",
                table: "Availabilities");
        }
    }
}
