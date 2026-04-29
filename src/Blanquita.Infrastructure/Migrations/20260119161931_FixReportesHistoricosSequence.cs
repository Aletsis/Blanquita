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
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"ReportesHistoricos\"', 'Id'), GREATEST(COALESCE((SELECT MAX(\"Id\") FROM \"ReportesHistoricos\"), 1), 1), (SELECT MAX(\"Id\") FROM \"ReportesHistoricos\") IS NOT NULL);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
