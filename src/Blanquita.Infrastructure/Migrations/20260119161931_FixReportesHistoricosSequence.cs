using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReportesHistoricosSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"ReportesHistoricos\"', 'Id'), COALESCE((SELECT MAX(\"Id\") FROM \"ReportesHistoricos\"), 0));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
