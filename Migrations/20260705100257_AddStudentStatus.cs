using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnilUniversity.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApplicationNo",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationNo",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
