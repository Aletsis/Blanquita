using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddLabelDesignEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiseñosEtiquetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnchoPuntos = table.Column<int>(type: "int", nullable: false),
                    AltoPuntos = table.Column<int>(type: "int", nullable: false),
                    MargenSuperior = table.Column<int>(type: "int", nullable: false),
                    MargenIzquierdo = table.Column<int>(type: "int", nullable: false),
                    Orientacion = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    TamañoFuenteNombre = table.Column<int>(type: "int", nullable: false),
                    TamañoFuenteCodigo = table.Column<int>(type: "int", nullable: false),
                    TamañoFuentePrecio = table.Column<int>(type: "int", nullable: false),
                    AlturaCodigoBarras = table.Column<int>(type: "int", nullable: false),
                    AnchoCodigoBarras = table.Column<int>(type: "int", nullable: false),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseñosEtiquetas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiseñosEtiquetas_Nombre",
                table: "DiseñosEtiquetas",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiseñosEtiquetas");
        }
    }
}
