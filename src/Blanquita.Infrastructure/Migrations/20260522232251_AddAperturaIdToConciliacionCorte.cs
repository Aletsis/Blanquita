using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAperturaIdToConciliacionCorte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AperturaId",
                table: "ConciliacionCortes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionCortes_AperturaId",
                table: "ConciliacionCortes",
                column: "AperturaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConciliacionCortes_AperturaId",
                table: "ConciliacionCortes");

            migrationBuilder.DropColumn(
                name: "AperturaId",
                table: "ConciliacionCortes");
        }
    }
}
