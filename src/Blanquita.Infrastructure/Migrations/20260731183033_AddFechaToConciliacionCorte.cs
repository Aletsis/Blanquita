using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaToConciliacionCorte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "ConciliacionCortes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE \"ConciliacionCortes\" SET \"Fecha\" = \"FechaCreacion\";");

            migrationBuilder.CreateIndex(
                name: "IX_ConciliacionCortes_Fecha",
                table: "ConciliacionCortes",
                column: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConciliacionCortes_Fecha",
                table: "ConciliacionCortes");

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "ConciliacionCortes");
        }
    }
}
