using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReferenceToSeatFromBookingDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingDetails_Seats_SeatId",
                table: "BookingDetails");

            migrationBuilder.DropIndex(
                name: "IX_BookingDetails_SeatId",
                table: "BookingDetails");

            migrationBuilder.DropColumn(
                name: "SeatId",
                table: "BookingDetails");

            migrationBuilder.AddColumn<string>(
                name: "SeatName",
                table: "BookingDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatName",
                table: "BookingDetails");

            migrationBuilder.AddColumn<int>(
                name: "SeatId",
                table: "BookingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_SeatId",
                table: "BookingDetails",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingDetails_Seats_SeatId",
                table: "BookingDetails",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id");
        }
    }
}
