using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddForeignKeyToCashRegisterBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove orphan rows that would violate the FK constraint
            migrationBuilder.Sql("DELETE FROM Cajas WHERE Sucursal NOT IN (SELECT Id FROM Sucursales)");

            migrationBuilder.AddForeignKey(
                name: "FK_Cajas_Sucursales_Sucursal",
                table: "Cajas",
                column: "Sucursal",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cajas_Sucursales_Sucursal",
                table: "Cajas");
        }
    }
}
