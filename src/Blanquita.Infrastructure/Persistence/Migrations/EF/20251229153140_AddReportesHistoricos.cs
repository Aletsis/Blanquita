using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddReportesHistoricos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesHistoricos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sucursal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalSistema = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCorteManual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesHistoricos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesReporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Caja = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Facturado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Devolucion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VentaGlobal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReporteHistoricoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesReporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesReporte_ReportesHistoricos_ReporteHistoricoId",
                        column: x => x.ReporteHistoricoId,
                        principalTable: "ReportesHistoricos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesReporte_ReporteHistoricoId",
                table: "DetallesReporte",
                column: "ReporteHistoricoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesReporte");

            migrationBuilder.DropTable(
                name: "ReportesHistoricos");
        }
    }
}
