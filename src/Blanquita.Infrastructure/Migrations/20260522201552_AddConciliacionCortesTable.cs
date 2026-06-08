using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConciliacionCortesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConciliacionCortes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sucursal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Caja = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cajero = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TotalRecolecciones = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EfectivoEntregado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalEfectivo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Banregio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Banbajio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalTarjetas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Devoluciones = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalEntregado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalEsperado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Diferencia = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConciliacionCortes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionCortes_Caja",
                table: "ConciliacionCortes",
                column: "Caja");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionCortes_FechaCreacion",
                table: "ConciliacionCortes",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionCortes_Sucursal",
                table: "ConciliacionCortes",
                column: "Sucursal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConciliacionCortes");
        }
    }
}
