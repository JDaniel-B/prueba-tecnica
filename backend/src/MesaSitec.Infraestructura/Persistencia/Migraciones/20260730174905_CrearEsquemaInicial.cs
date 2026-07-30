using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MesaSitec.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CrearEsquemaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SlaHoras = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.UniqueConstraint("AK_Categorias_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.CheckConstraint("CK_Categorias_SlaHoras_Positivo", "\"SlaHoras\" > 0");
                    table.ForeignKey(
                        name: "FK_Categorias_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false, collation: "NOCASE"),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.UniqueConstraint("AK_Usuarios_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_Usuarios_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Prioridad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgenteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaLimiteSla = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoResolucion = table.Column<string>(type: "TEXT", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.CheckConstraint("CK_Solicitudes_Descripcion_Longitud", "length(trim(\"Descripcion\")) BETWEEN 10 AND 4000");
                    table.CheckConstraint("CK_Solicitudes_Titulo_Longitud", "length(trim(\"Titulo\")) BETWEEN 5 AND 120");
                    table.ForeignKey(
                        name: "FK_Solicitudes_Categorias_TenantId_CategoriaId",
                        columns: x => new { x.TenantId, x.CategoriaId },
                        principalTable: "Categorias",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_TenantId_AgenteId",
                        columns: x => new { x.TenantId, x.AgenteId },
                        principalTable: "Usuarios",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_TenantId_SolicitanteId",
                        columns: x => new { x.TenantId, x.SolicitanteId },
                        principalTable: "Usuarios",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_TenantId_Activo",
                table: "Categorias",
                columns: new[] { "TenantId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_AgenteId",
                table: "Solicitudes",
                columns: new[] { "TenantId", "AgenteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_CategoriaId",
                table: "Solicitudes",
                columns: new[] { "TenantId", "CategoriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_Codigo",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_Estado",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_FechaCreacion",
                table: "Solicitudes",
                columns: new[] { "TenantId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_Prioridad",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Prioridad" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_SolicitanteId",
                table: "Solicitudes",
                columns: new[] { "TenantId", "SolicitanteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_Rol_Activo",
                table: "Usuarios",
                columns: new[] { "TenantId", "Rol", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
