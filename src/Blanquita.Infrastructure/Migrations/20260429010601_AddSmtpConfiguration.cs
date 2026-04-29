using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmtpConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnableSsl",
                table: "Configuracion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromEmail",
                table: "Configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromName",
                table: "Configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "Configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Configuracion",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SmtpServer",
                table: "Configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SmtpUser",
                table: "Configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpEnableSsl",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "SmtpFromEmail",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "SmtpFromName",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "SmtpServer",
                table: "Configuracion");

            migrationBuilder.DropColumn(
                name: "SmtpUser",
                table: "Configuracion");
        }
    }
}
