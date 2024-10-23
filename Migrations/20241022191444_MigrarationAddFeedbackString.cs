using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital_Managment_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrarationAddFeedbackString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Appointments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Appointments");
        }
    }
}
