using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddjustPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ConcessionOrders_AspNetUsers_UserId",
                table: "ConcessionOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ConcessionOrders_Bookings_BookingId",
                table: "ConcessionOrders");

            migrationBuilder.DropIndex(
                name: "IX_ConcessionOrders_BookingId",
                table: "ConcessionOrders");

            migrationBuilder.DropIndex(
                name: "IX_ConcessionOrders_UserId",
                table: "ConcessionOrders");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "ConcessionOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ConcessionOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "payments",
                newName: "SessionId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_UserId",
                table: "payments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_AspNetUsers_UserId",
                table: "payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_AspNetUsers_UserId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_UserId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "payments",
                newName: "TransactionId");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "ConcessionOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ConcessionOrders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ConcessionOrders_BookingId",
                table: "ConcessionOrders",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcessionOrders_UserId",
                table: "ConcessionOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConcessionOrders_AspNetUsers_UserId",
                table: "ConcessionOrders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConcessionOrders_Bookings_BookingId",
                table: "ConcessionOrders",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");
        }
    }
}
