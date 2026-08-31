using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConciliacionSalidasAndWhatsAppFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWhatsAppEnabled",
                table: "Configuracion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "ConciliacionCortes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "ConciliacionCortes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalidasEfectivo",
                table: "ConciliacionCortes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TerminalesJson",
                table: "ConciliacionCortes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Usuario",
                table: "ConciliacionCortes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConciliacionSalidasEfectivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConciliacionCorteId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    QuienAutoriza = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConciliacionSalidasEfectivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConciliacionSalidasEfectivo_ConciliacionCortes_Conciliacion~",
                        column: x => x.ConciliacionCorteId,
                        principalTable: "ConciliacionCortes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionSalidasEfectivo_ConciliacionCorteId",
                table: "ConciliacionSalidasEfectivo",
                column: "ConciliacionCorteId");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionSalidasEfectivo_FechaCreacion",
                table: "ConciliacionSalidasEfectivo",
                column: "FechaCreacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConciliacionSalidasEfectivo");

            migrationBuilder.DropColumn(
                name: "IsWhatsAppEnabled",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "ConciliacionCortes");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "ConciliacionCortes");

            migrationBuilder.DropColumn(
                name: "SalidasEfectivo",
                table: "ConciliacionCortes");

            migrationBuilder.DropColumn(
                name: "TerminalesJson",
                table: "ConciliacionCortes");

            migrationBuilder.DropColumn(
                name: "Usuario",
                table: "ConciliacionCortes");
        }
    }
}
