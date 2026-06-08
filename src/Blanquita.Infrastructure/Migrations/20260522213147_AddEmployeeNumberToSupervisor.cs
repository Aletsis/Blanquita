using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeNumberToSupervisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumNomina",
                table: "Encargadas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE \"Encargadas\" SET \"NumNomina\" = \"Id\" WHERE \"NumNomina\" = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Encargadas_NumNomina",
                table: "Encargadas",
                column: "NumNomina",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Encargadas_NumNomina",
                table: "Encargadas");

            migrationBuilder.DropColumn(
                name: "NumNomina",
                table: "Encargadas");
        }
    }
}
