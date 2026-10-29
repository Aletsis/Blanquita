using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToSentInvoiceLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar registros duplicados (conservando el primero) para que el índice único pueda crearse
            migrationBuilder.Sql(@"
                DELETE FROM ""SentInvoiceLogs"" a
                USING ""SentInvoiceLogs"" b
                WHERE a.""ClientCode"" = b.""ClientCode""
                  AND a.""FileName"" = b.""FileName""
                  AND a.""Id"" > b.""Id"";");

            migrationBuilder.CreateIndex(
                name: "IX_SentInvoiceLogs_ClientCode_FileName",
                table: "SentInvoiceLogs",
                columns: new[] { "ClientCode", "FileName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SentInvoiceLogs_ClientCode_FileName",
                table: "SentInvoiceLogs");
        }
    }
}
