using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class ModificadoHorariosClasificador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Horario_Clasificador_id_dia_semana",
                table: "Horario");

            migrationBuilder.DropIndex(
                name: "IX_Horario_id_dia_semana",
                table: "Horario");

            migrationBuilder.DropIndex(
                name: "IX_Horario_id_trabajador_id_dia_semana",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "id_dia_semana",
                table: "Horario");

            migrationBuilder.AddColumn<string>(
                name: "dia_semana",
                table: "Horario",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Horario_id_trabajador_dia_semana",
                table: "Horario",
                columns: new[] { "id_trabajador", "dia_semana" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Horario_id_trabajador_dia_semana",
                table: "Horario");

            migrationBuilder.DropColumn(
                name: "dia_semana",
                table: "Horario");

            migrationBuilder.AddColumn<int>(
                name: "id_dia_semana",
                table: "Horario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Horario_id_dia_semana",
                table: "Horario",
                column: "id_dia_semana");

            migrationBuilder.CreateIndex(
                name: "IX_Horario_id_trabajador_id_dia_semana",
                table: "Horario",
                columns: new[] { "id_trabajador", "id_dia_semana" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Horario_Clasificador_id_dia_semana",
                table: "Horario",
                column: "id_dia_semana",
                principalTable: "Clasificador",
                principalColumn: "id_clasificador",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
