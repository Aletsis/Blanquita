using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class FixDecimalComputedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadTotal",
                table: "Recolecciones",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "CAST([Mil] * 1000 + [Quinientos] * 500 + [Doscientos] * 200 + [Cien] * 100 + [Cincuenta] * 50 + [Veinte] * 20 AS decimal(18,2))",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "[Mil] * 1000 + [Quinientos] * 500 + [Doscientos] * 200 + [Cien] * 100 + [Cincuenta] * 50 + [Veinte] * 20");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadTotal",
                table: "Recolecciones",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "[Mil] * 1000 + [Quinientos] * 500 + [Doscientos] * 200 + [Cien] * 100 + [Cincuenta] * 50 + [Veinte] * 20",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "CAST([Mil] * 1000 + [Quinientos] * 500 + [Doscientos] * 200 + [Cien] * 100 + [Cincuenta] * 50 + [Veinte] * 20 AS decimal(18,2))");
        }
    }
}
