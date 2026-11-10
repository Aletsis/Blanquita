using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUmbralAlertaEfectivoCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UmbralAlertaEfectivoCaja",
                table: "Configuracion",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 8000m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UmbralAlertaEfectivoCaja",
                table: "Configuracion");
        }
    }
}
