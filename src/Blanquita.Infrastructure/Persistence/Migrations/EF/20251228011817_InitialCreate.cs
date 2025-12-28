using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IpImpresora = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Sucursal = table.Column<int>(type: "int", nullable: false),
                    Ultima = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cajeras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumNomina = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sucursal = table.Column<int>(type: "int", nullable: false),
                    Edo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajeras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cortes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalM = table.Column<int>(type: "int", nullable: false),
                    TotalQ = table.Column<int>(type: "int", nullable: false),
                    TotalD = table.Column<int>(type: "int", nullable: false),
                    TotalC = table.Column<int>(type: "int", nullable: false),
                    TotalCi = table.Column<int>(type: "int", nullable: false),
                    TotalV = table.Column<int>(type: "int", nullable: false),
                    TotalTira = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTarjetas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Caja = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Encargada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cajera = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sucursal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GranTotal = table.Column<int>(type: "int", nullable: false, computedColumnSql: "[TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + CAST([TotalTira] AS INT) + CAST([TotalTarjetas] AS INT)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cortes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Encargadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sucursal = table.Column<int>(type: "int", nullable: false),
                    Edo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encargadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recolecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mil = table.Column<int>(type: "int", nullable: false),
                    Quinientos = table.Column<int>(type: "int", nullable: false),
                    Doscientos = table.Column<int>(type: "int", nullable: false),
                    Cien = table.Column<int>(type: "int", nullable: false),
                    Cincuenta = table.Column<int>(type: "int", nullable: false),
                    Veinte = table.Column<int>(type: "int", nullable: false),
                    Caja = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cajera = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Encargada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Folio = table.Column<int>(type: "int", nullable: false),
                    Corte = table.Column<bool>(type: "bit", nullable: false),
                    CantidadTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[Mil] * 1000 + [Quinientos] * 500 + [Doscientos] * 200 + [Cien] * 100 + [Cincuenta] * 50 + [Veinte] * 20")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recolecciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_Nombre",
                table: "Cajas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cajeras_NumNomina",
                table: "Cajeras",
                column: "NumNomina",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Folio",
                table: "Recolecciones",
                column: "Folio",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "Cajeras");

            migrationBuilder.DropTable(
                name: "Cortes");

            migrationBuilder.DropTable(
                name: "Encargadas");

            migrationBuilder.DropTable(
                name: "Recolecciones");
        }
    }
}
