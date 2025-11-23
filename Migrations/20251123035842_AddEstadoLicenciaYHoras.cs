using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoLicenciaYHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licencia_Clasificador_id_tipo_licencia",
                table: "Licencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Licencia_Trabajador_id_trabajador",
                table: "Licencia");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_aprobacion",
                table: "Licencia",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_estado_licencia",
                table: "Licencia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Licencia_id_estado_licencia",
                table: "Licencia",
                column: "id_estado_licencia");

            migrationBuilder.AddForeignKey(
                name: "FK_Licencia_Clasificador_id_estado_licencia",
                table: "Licencia",
                column: "id_estado_licencia",
                principalTable: "Clasificador",
                principalColumn: "id_clasificador",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Licencia_Clasificador_id_tipo_licencia",
                table: "Licencia",
                column: "id_tipo_licencia",
                principalTable: "Clasificador",
                principalColumn: "id_clasificador",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Licencia_Trabajador_id_trabajador",
                table: "Licencia",
                column: "id_trabajador",
                principalTable: "Trabajador",
                principalColumn: "id_trabajador",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Licencia_Clasificador_id_estado_licencia",
                table: "Licencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Licencia_Clasificador_id_tipo_licencia",
                table: "Licencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Licencia_Trabajador_id_trabajador",
                table: "Licencia");

            migrationBuilder.DropIndex(
                name: "IX_Licencia_id_estado_licencia",
                table: "Licencia");

            migrationBuilder.DropColumn(
                name: "fecha_aprobacion",
                table: "Licencia");

            migrationBuilder.DropColumn(
                name: "id_estado_licencia",
                table: "Licencia");

            migrationBuilder.AddForeignKey(
                name: "FK_Licencia_Clasificador_id_tipo_licencia",
                table: "Licencia",
                column: "id_tipo_licencia",
                principalTable: "Clasificador",
                principalColumn: "id_clasificador",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Licencia_Trabajador_id_trabajador",
                table: "Licencia",
                column: "id_trabajador",
                principalTable: "Trabajador",
                principalColumn: "id_trabajador",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
