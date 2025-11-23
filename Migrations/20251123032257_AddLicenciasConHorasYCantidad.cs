using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenciasConHorasYCantidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Solicitud_Clasificador_id_tipo_solicitud",
                table: "Solicitud");

            migrationBuilder.DropIndex(
                name: "IX_Solicitud_id_tipo_solicitud",
                table: "Solicitud");

            migrationBuilder.DropColumn(
                name: "id_tipo_solicitud",
                table: "Solicitud");

            migrationBuilder.CreateTable(
                name: "Licencia",
                columns: table => new
                {
                    id_licencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_tipo_licencia = table.Column<int>(type: "int", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: false),
                    hora_inicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    hora_fin = table.Column<TimeSpan>(type: "time", nullable: false),
                    cantidad_jornadas = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    archivo_justificativo = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licencia", x => x.id_licencia);
                    table.ForeignKey(
                        name: "FK_Licencia_Clasificador_id_tipo_licencia",
                        column: x => x.id_tipo_licencia,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Licencia_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Licencia_id_tipo_licencia",
                table: "Licencia",
                column: "id_tipo_licencia");

            migrationBuilder.CreateIndex(
                name: "IX_Licencia_id_trabajador",
                table: "Licencia",
                column: "id_trabajador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Licencia");

            migrationBuilder.AddColumn<int>(
                name: "id_tipo_solicitud",
                table: "Solicitud",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_id_tipo_solicitud",
                table: "Solicitud",
                column: "id_tipo_solicitud");

            migrationBuilder.AddForeignKey(
                name: "FK_Solicitud_Clasificador_id_tipo_solicitud",
                table: "Solicitud",
                column: "id_tipo_solicitud",
                principalTable: "Clasificador",
                principalColumn: "id_clasificador",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
