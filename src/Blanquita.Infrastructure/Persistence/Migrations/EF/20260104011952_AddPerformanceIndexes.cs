using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_Fecha",
                table: "ReportesHistoricos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_FechaGeneracion",
                table: "ReportesHistoricos",
                column: "FechaGeneracion");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_Sucursal_Fecha",
                table: "ReportesHistoricos",
                columns: new[] { "Sucursal", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Caja",
                table: "Recolecciones",
                column: "Caja");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Caja_FechaHora_Corte",
                table: "Recolecciones",
                columns: new[] { "Caja", "FechaHora", "Corte" });

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Corte",
                table: "Recolecciones",
                column: "Corte");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_FechaHora",
                table: "Recolecciones",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_Caja",
                table: "Cortes",
                column: "Caja");

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_FechaHora",
                table: "Cortes",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_FechaHora_Caja",
                table: "Cortes",
                columns: new[] { "FechaHora", "Caja" });

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_Sucursal",
                table: "Cortes",
                column: "Sucursal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportesHistoricos_Fecha",
                table: "ReportesHistoricos");

            migrationBuilder.DropIndex(
                name: "IX_ReportesHistoricos_FechaGeneracion",
                table: "ReportesHistoricos");

            migrationBuilder.DropIndex(
                name: "IX_ReportesHistoricos_Sucursal_Fecha",
                table: "ReportesHistoricos");

            migrationBuilder.DropIndex(
                name: "IX_Recolecciones_Caja",
                table: "Recolecciones");

            migrationBuilder.DropIndex(
                name: "IX_Recolecciones_Caja_FechaHora_Corte",
                table: "Recolecciones");

            migrationBuilder.DropIndex(
                name: "IX_Recolecciones_Corte",
                table: "Recolecciones");

            migrationBuilder.DropIndex(
                name: "IX_Recolecciones_FechaHora",
                table: "Recolecciones");

            migrationBuilder.DropIndex(
                name: "IX_Cortes_Caja",
                table: "Cortes");

            migrationBuilder.DropIndex(
                name: "IX_Cortes_FechaHora",
                table: "Cortes");

            migrationBuilder.DropIndex(
                name: "IX_Cortes_FechaHora_Caja",
                table: "Cortes");

            migrationBuilder.DropIndex(
                name: "IX_Cortes_Sucursal",
                table: "Cortes");
        }
    }
}
