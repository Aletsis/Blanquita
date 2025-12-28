using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class UpdateCashCutGranTotalFormula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "GranTotal",
                table: "Cortes",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 AS decimal(18,2))",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + [TotalTira] + [TotalTarjetas] AS decimal(18,2))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "GranTotal",
                table: "Cortes",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + [TotalTira] + [TotalTarjetas] AS decimal(18,2))",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 AS decimal(18,2))");
        }
    }
}
