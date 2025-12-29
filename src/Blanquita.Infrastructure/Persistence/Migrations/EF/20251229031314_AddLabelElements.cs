using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Persistence.Migrations.EF
{
    /// <inheritdoc />
    public partial class AddLabelElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElementosEtiqueta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseñoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    XMm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YMm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TamañoFuente = table.Column<int>(type: "int", nullable: false),
                    AltoMm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnchoBarra = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementosEtiqueta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElementosEtiqueta_DiseñosEtiquetas_DiseñoId",
                        column: x => x.DiseñoId,
                        principalTable: "DiseñosEtiquetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElementosEtiqueta_DiseñoId",
                table: "ElementosEtiqueta",
                column: "DiseñoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElementosEtiqueta");
        }
    }
}
