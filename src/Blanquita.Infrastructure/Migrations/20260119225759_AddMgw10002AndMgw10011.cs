using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMgw10002AndMgw10011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mgw10002Path",
                table: "Configuracion",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mgw10011Path",
                table: "Configuracion",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mgw10002Path",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "Mgw10011Path",
                table: "Configuracion");
        }
    }
}
