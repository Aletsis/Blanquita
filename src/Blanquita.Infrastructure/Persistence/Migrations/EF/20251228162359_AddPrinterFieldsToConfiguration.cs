using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddPrinterFieldsToConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Printer2Ip",
                table: "Configuracion",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Printer2Name",
                table: "Configuracion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Printer2Port",
                table: "Configuracion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrinterIp",
                table: "Configuracion",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrinterName",
                table: "Configuracion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PrinterPort",
                table: "Configuracion",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Printer2Ip",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "Printer2Name",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "Printer2Port",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "PrinterIp",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "PrinterName",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "PrinterPort",
                table: "Configuracion");
        }
    }
}
