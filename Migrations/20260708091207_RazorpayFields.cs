using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnilUniversity.Migrations
{
    /// <inheritdoc />
    public partial class RazorpayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RazorpayOrderId",
                table: "StudentPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazorpayPaymentId",
                table: "StudentPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazorpaySignature",
                table: "StudentPayments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayOrderId",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "RazorpayPaymentId",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "RazorpaySignature",
                table: "StudentPayments");
        }
    }
}
