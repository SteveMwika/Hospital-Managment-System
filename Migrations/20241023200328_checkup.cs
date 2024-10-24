using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital_Managment_System.Migrations
{
    /// <inheritdoc />
    public partial class checkup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabTests_Appointments_AppointmentId",
                table: "LabTests");

            migrationBuilder.RenameColumn(
                name: "Result",
                table: "LabTests",
                newName: "TestResult");

            migrationBuilder.AlterColumn<int>(
                name: "TestName",
                table: "LabTests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "LabTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Billings",
                type: "Date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Billings",
                type: "Date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Billings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_LabTests_Appointments_AppointmentId",
                table: "LabTests",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabTests_Appointments_AppointmentId",
                table: "LabTests");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "LabTests");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Billings");

            migrationBuilder.RenameColumn(
                name: "TestResult",
                table: "LabTests",
                newName: "Result");

            migrationBuilder.AlterColumn<string>(
                name: "TestName",
                table: "LabTests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_LabTests_Appointments_AppointmentId",
                table: "LabTests",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
