using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnilUniversity.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSeats",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSeats",
                table: "Courses");
        }
    }
}
