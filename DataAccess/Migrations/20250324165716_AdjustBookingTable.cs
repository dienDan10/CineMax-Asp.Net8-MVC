using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdjustBookingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_AspNetUsers_UserId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_Bookings_BookingId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_ConcessionOrders_ConcessionOrderId",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_payments_UserId",
                table: "Payments",
                newName: "IX_Payments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_ConcessionOrderId",
                table: "Payments",
                newName: "IX_Payments_ConcessionOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_BookingId",
                table: "Payments",
                newName: "IX_Payments_BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ConcessionOrders_ConcessionOrderId",
                table: "Payments",
                column: "ConcessionOrderId",
                principalTable: "ConcessionOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ConcessionOrders_ConcessionOrderId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payments");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_UserId",
                table: "payments",
                newName: "IX_payments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_ConcessionOrderId",
                table: "payments",
                newName: "IX_payments_ConcessionOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BookingId",
                table: "payments",
                newName: "IX_payments_BookingId");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_AspNetUsers_UserId",
                table: "payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_Bookings_BookingId",
                table: "payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_ConcessionOrders_ConcessionOrderId",
                table: "payments",
                column: "ConcessionOrderId",
                principalTable: "ConcessionOrders",
                principalColumn: "Id");
        }
    }
}
