using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddForeignKeyToCashierAndSupervisorBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove orphan rows for Cashiers (Cajeras)
            migrationBuilder.Sql("DELETE FROM Cajeras WHERE Sucursal NOT IN (SELECT Id FROM Sucursales)");
            
            // Remove orphan rows for Supervisors (Encargadas)
            migrationBuilder.Sql("DELETE FROM Encargadas WHERE Sucursal NOT IN (SELECT Id FROM Sucursales)");

            // Add FK for Cashiers
            migrationBuilder.AddForeignKey(
                name: "FK_Cajeras_Sucursales_Sucursal",
                table: "Cajeras",
                column: "Sucursal",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Add FK for Supervisors
            migrationBuilder.AddForeignKey(
                name: "FK_Encargadas_Sucursales_Sucursal",
                table: "Encargadas",
                column: "Sucursal",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cajeras_Sucursales_Sucursal",
                table: "Cajeras");

            migrationBuilder.DropForeignKey(
                name: "FK_Encargadas_Sucursales_Sucursal",
                table: "Encargadas");
        }
    }
}
