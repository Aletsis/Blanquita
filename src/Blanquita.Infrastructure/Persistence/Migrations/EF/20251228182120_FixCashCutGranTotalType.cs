using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class FixCashCutGranTotalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "GranTotal",
                table: "Cortes",
                type: "decimal(18,2)",
                nullable: false,
                computedColumnSql: "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + [TotalTira] + [TotalTarjetas] AS decimal(18,2))",
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "[TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + CAST([TotalTira] AS INT) + CAST([TotalTarjetas] AS INT)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GranTotal",
                table: "Cortes",
                type: "int",
                nullable: false,
                computedColumnSql: "[TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + CAST([TotalTira] AS INT) + CAST([TotalTarjetas] AS INT)",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldComputedColumnSql: "CAST([TotalM] * 1000 + [TotalQ] * 500 + [TotalD] * 200 + [TotalC] * 100 + [TotalCi] * 50 + [TotalV] * 20 + [TotalTira] + [TotalTarjetas] AS decimal(18,2))");
        }
    }
}
