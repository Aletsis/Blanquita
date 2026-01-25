using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparateCardTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalTarjetas",
                table: "Cortes",
                newName: "TotalBanregio");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBanbajio",
                table: "Cortes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalBanbajio",
                table: "Cortes");

            migrationBuilder.RenameColumn(
                name: "TotalBanregio",
                table: "Cortes",
                newName: "TotalTarjetas");
        }
    }
}
