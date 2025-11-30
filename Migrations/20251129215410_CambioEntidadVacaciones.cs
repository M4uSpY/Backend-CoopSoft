using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class CambioEntidadVacaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Solicitud");

            migrationBuilder.CreateTable(
                name: "Vacacion",
                columns: table => new
                {
                    id_vacacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_estado_solicitud = table.Column<int>(type: "int", nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_solicitud = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_aprobacion = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacacion", x => x.id_vacacion);
                    table.ForeignKey(
                        name: "FK_Vacacion_Clasificador_id_estado_solicitud",
                        column: x => x.id_estado_solicitud,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vacacion_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vacacion_id_estado_solicitud",
                table: "Vacacion",
                column: "id_estado_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_Vacacion_id_trabajador",
                table: "Vacacion",
                column: "id_trabajador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vacacion");

            migrationBuilder.CreateTable(
                name: "Solicitud",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_estado_solicitud = table.Column<int>(type: "int", nullable: false),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    fecha_aprobacion = table.Column<DateTime>(type: "date", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_solicitud = table.Column<DateTime>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitud", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK_Solicitud_Clasificador_id_estado_solicitud",
                        column: x => x.id_estado_solicitud,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_id_estado_solicitud",
                table: "Solicitud",
                column: "id_estado_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_id_trabajador",
                table: "Solicitud",
                column: "id_trabajador");
        }
    }
}
