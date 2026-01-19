using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReportesHistoricosSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportesHistoricos_Sucursal_Fecha",
                table: "ReportesHistoricos");

            migrationBuilder.RenameColumn(
                name: "Sucursal",
                table: "ReportesHistoricos",
                newName: "SucursalCodigo");

            migrationBuilder.AddColumn<string>(
                name: "SucursalNombre",
                table: "ReportesHistoricos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Populate SucursalNombre based on SucursalCodigo for existing records
            migrationBuilder.Sql("UPDATE \"ReportesHistoricos\" SET \"SucursalNombre\" = CASE \"SucursalCodigo\" WHEN 'HIM' THEN 'Himno' WHEN 'POZ' THEN 'Pozos' WHEN 'SOL' THEN 'Soledad' WHEN 'SAU' THEN 'Saucito' WHEN 'CHA' THEN 'Chapultepec' ELSE \"SucursalCodigo\" END");


            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_SucursalCodigo",
                table: "ReportesHistoricos",
                column: "SucursalCodigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportesHistoricos_SucursalCodigo",
                table: "ReportesHistoricos");

            migrationBuilder.DropColumn(
                name: "SucursalNombre",
                table: "ReportesHistoricos");

            migrationBuilder.RenameColumn(
                name: "SucursalCodigo",
                table: "ReportesHistoricos",
                newName: "Sucursal");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_Sucursal_Fecha",
                table: "ReportesHistoricos",
                columns: new[] { "Sucursal", "Fecha" });
        }
    }
}
