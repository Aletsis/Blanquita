using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blanquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Serie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IpImpresora = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Sucursal = table.Column<int>(type: "integer", nullable: false),
                    Ultima = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cajeras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumNomina = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sucursal = table.Column<int>(type: "integer", nullable: false),
                    Edo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajeras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pos10041Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Pos10042Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Mgw10008Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Mgw10005Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PrinterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrinterIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrinterPort = table.Column<int>(type: "integer", nullable: false),
                    Printer2Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Printer2Ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Printer2Port = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cortes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TotalM = table.Column<int>(type: "integer", nullable: false),
                    TotalQ = table.Column<int>(type: "integer", nullable: false),
                    TotalD = table.Column<int>(type: "integer", nullable: false),
                    TotalC = table.Column<int>(type: "integer", nullable: false),
                    TotalCi = table.Column<int>(type: "integer", nullable: false),
                    TotalV = table.Column<int>(type: "integer", nullable: false),
                    TotalTira = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalTarjetas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Caja = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Encargada = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cajera = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sucursal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GranTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false, computedColumnSql: "CAST(\"TotalM\" * 1000 + \"TotalQ\" * 500 + \"TotalD\" * 200 + \"TotalC\" * 100 + \"TotalCi\" * 50 + \"TotalV\" * 20 AS decimal(18,2))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cortes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiseñosEtiquetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AnchoMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AltoMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MargenSuperiorMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MargenIzquierdoMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Orientacion = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    TamañoFuenteNombre = table.Column<int>(type: "integer", nullable: false),
                    TamañoFuenteCodigo = table.Column<int>(type: "integer", nullable: false),
                    TamañoFuentePrecio = table.Column<int>(type: "integer", nullable: false),
                    AlturaCodigoBarrasMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AnchoCodigoBarras = table.Column<int>(type: "integer", nullable: false),
                    EsPredeterminado = table.Column<bool>(type: "boolean", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiseñosEtiquetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Encargadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sucursal = table.Column<int>(type: "integer", nullable: false),
                    Edo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encargadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Impresoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Puerto = table.Column<int>(type: "integer", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    Dpi = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impresoras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recolecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mil = table.Column<int>(type: "integer", nullable: false),
                    Quinientos = table.Column<int>(type: "integer", nullable: false),
                    Doscientos = table.Column<int>(type: "integer", nullable: false),
                    Cien = table.Column<int>(type: "integer", nullable: false),
                    Cincuenta = table.Column<int>(type: "integer", nullable: false),
                    Veinte = table.Column<int>(type: "integer", nullable: false),
                    Caja = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cajera = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Encargada = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Folio = table.Column<int>(type: "integer", nullable: false),
                    Corte = table.Column<bool>(type: "boolean", nullable: false),
                    CantidadTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false, computedColumnSql: "CAST(\"Mil\" * 1000 + \"Quinientos\" * 500 + \"Doscientos\" * 200 + \"Cien\" * 100 + \"Cincuenta\" * 50 + \"Veinte\" * 20 AS decimal(18,2))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recolecciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportesHistoricos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sucursal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalSistema = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCorteManual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesHistoricos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SeriesCliente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeriesGlobal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SeriesDevolucion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElementosEtiqueta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiseñoId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    XMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    YMm = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Contenido = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TamañoFuente = table.Column<int>(type: "integer", nullable: false),
                    AltoMm = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AnchoBarra = table.Column<int>(type: "integer", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "DetallesReporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Caja = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Facturado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Devolucion = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VentaGlobal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReporteHistoricoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesReporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesReporte_ReportesHistoricos_ReporteHistoricoId",
                        column: x => x.ReporteHistoricoId,
                        principalTable: "ReportesHistoricos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_Nombre",
                table: "Cajas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cajeras_NumNomina",
                table: "Cajeras",
                column: "NumNomina",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_Caja",
                table: "Cortes",
                column: "Caja");

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_FechaHora",
                table: "Cortes",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_FechaHora_Caja",
                table: "Cortes",
                columns: new[] { "FechaHora", "Caja" });

            migrationBuilder.CreateIndex(
                name: "IX_Cortes_Sucursal",
                table: "Cortes",
                column: "Sucursal");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesReporte_ReporteHistoricoId",
                table: "DetallesReporte",
                column: "ReporteHistoricoId");

            migrationBuilder.CreateIndex(
                name: "IX_DiseñosEtiquetas_Nombre",
                table: "DiseñosEtiquetas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElementosEtiqueta_DiseñoId",
                table: "ElementosEtiqueta",
                column: "DiseñoId");

            migrationBuilder.CreateIndex(
                name: "IX_Impresoras_Nombre",
                table: "Impresoras",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Caja",
                table: "Recolecciones",
                column: "Caja");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Caja_FechaHora_Corte",
                table: "Recolecciones",
                columns: new[] { "Caja", "FechaHora", "Corte" });

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_Corte",
                table: "Recolecciones",
                column: "Corte");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_FechaHora",
                table: "Recolecciones",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_Fecha",
                table: "ReportesHistoricos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_FechaGeneracion",
                table: "ReportesHistoricos",
                column: "FechaGeneracion");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHistoricos_Sucursal_Fecha",
                table: "ReportesHistoricos",
                columns: new[] { "Sucursal", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "Cajeras");

            migrationBuilder.DropTable(
                name: "Configuracion");

            migrationBuilder.DropTable(
                name: "Cortes");

            migrationBuilder.DropTable(
                name: "DetallesReporte");

            migrationBuilder.DropTable(
                name: "ElementosEtiqueta");

            migrationBuilder.DropTable(
                name: "Encargadas");

            migrationBuilder.DropTable(
                name: "Impresoras");

            migrationBuilder.DropTable(
                name: "Recolecciones");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ReportesHistoricos");

            migrationBuilder.DropTable(
                name: "DiseñosEtiquetas");
        }
    }
}
