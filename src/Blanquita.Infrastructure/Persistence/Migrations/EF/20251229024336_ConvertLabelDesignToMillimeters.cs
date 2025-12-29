using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class ConvertLabelDesignToMillimeters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltoPuntos",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "AlturaCodigoBarras",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "AnchoPuntos",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "MargenIzquierdo",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "MargenSuperior",
                table: "DiseñosEtiquetas");

            migrationBuilder.AddColumn<decimal>(
                name: "AltoMm",
                table: "DiseñosEtiquetas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AlturaCodigoBarrasMm",
                table: "DiseñosEtiquetas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AnchoMm",
                table: "DiseñosEtiquetas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MargenIzquierdoMm",
                table: "DiseñosEtiquetas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MargenSuperiorMm",
                table: "DiseñosEtiquetas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltoMm",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "AlturaCodigoBarrasMm",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "AnchoMm",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "MargenIzquierdoMm",
                table: "DiseñosEtiquetas");

            migrationBuilder.DropColumn(
                name: "MargenSuperiorMm",
                table: "DiseñosEtiquetas");

            migrationBuilder.AddColumn<int>(
                name: "AltoPuntos",
                table: "DiseñosEtiquetas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AlturaCodigoBarras",
                table: "DiseñosEtiquetas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnchoPuntos",
                table: "DiseñosEtiquetas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MargenIzquierdo",
                table: "DiseñosEtiquetas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MargenSuperior",
                table: "DiseñosEtiquetas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
