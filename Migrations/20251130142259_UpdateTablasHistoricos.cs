using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablasHistoricos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "apartados_modificados",
                table: "Historico_Usuario");

            migrationBuilder.DropColumn(
                name: "apartados_modificados",
                table: "Historico_Trabajador");

            migrationBuilder.DropColumn(
                name: "apartados_modificados",
                table: "Historico_Falta");

            migrationBuilder.RenameColumn(
                name: "apartados_modificados",
                table: "Historico_Persona",
                newName: "valor_anterior");

            migrationBuilder.AddColumn<string>(
                name: "campo",
                table: "Historico_Usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "valor_actual",
                table: "Historico_Usuario",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valor_anterior",
                table: "Historico_Usuario",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "accion",
                table: "Historico_Trabajador_Planilla",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "campo",
                table: "Historico_Trabajador_Planilla",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "valor_actual",
                table: "Historico_Trabajador_Planilla",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valor_anterior",
                table: "Historico_Trabajador_Planilla",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "campo",
                table: "Historico_Trabajador",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "valor_actual",
                table: "Historico_Trabajador",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valor_anterior",
                table: "Historico_Trabajador",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "accion",
                table: "Historico_Planilla",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "campo",
                table: "Historico_Planilla",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "valor_actual",
                table: "Historico_Planilla",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valor_anterior",
                table: "Historico_Planilla",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "campo",
                table: "Historico_Persona",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "valor_actual",
                table: "Historico_Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "campo",
                table: "Historico_Falta",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "valor_actual",
                table: "Historico_Falta",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valor_anterior",
                table: "Historico_Falta",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "campo",
                table: "Historico_Usuario");

            migrationBuilder.DropColumn(
                name: "valor_actual",
                table: "Historico_Usuario");

            migrationBuilder.DropColumn(
                name: "valor_anterior",
                table: "Historico_Usuario");

            migrationBuilder.DropColumn(
                name: "accion",
                table: "Historico_Trabajador_Planilla");

            migrationBuilder.DropColumn(
                name: "campo",
                table: "Historico_Trabajador_Planilla");

            migrationBuilder.DropColumn(
                name: "valor_actual",
                table: "Historico_Trabajador_Planilla");

            migrationBuilder.DropColumn(
                name: "valor_anterior",
                table: "Historico_Trabajador_Planilla");

            migrationBuilder.DropColumn(
                name: "campo",
                table: "Historico_Trabajador");

            migrationBuilder.DropColumn(
                name: "valor_actual",
                table: "Historico_Trabajador");

            migrationBuilder.DropColumn(
                name: "valor_anterior",
                table: "Historico_Trabajador");

            migrationBuilder.DropColumn(
                name: "accion",
                table: "Historico_Planilla");

            migrationBuilder.DropColumn(
                name: "campo",
                table: "Historico_Planilla");

            migrationBuilder.DropColumn(
                name: "valor_actual",
                table: "Historico_Planilla");

            migrationBuilder.DropColumn(
                name: "valor_anterior",
                table: "Historico_Planilla");

            migrationBuilder.DropColumn(
                name: "campo",
                table: "Historico_Persona");

            migrationBuilder.DropColumn(
                name: "valor_actual",
                table: "Historico_Persona");

            migrationBuilder.DropColumn(
                name: "campo",
                table: "Historico_Falta");

            migrationBuilder.DropColumn(
                name: "valor_actual",
                table: "Historico_Falta");

            migrationBuilder.DropColumn(
                name: "valor_anterior",
                table: "Historico_Falta");

            migrationBuilder.RenameColumn(
                name: "valor_anterior",
                table: "Historico_Persona",
                newName: "apartados_modificados");

            migrationBuilder.AddColumn<string>(
                name: "apartados_modificados",
                table: "Historico_Usuario",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "apartados_modificados",
                table: "Historico_Trabajador",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "apartados_modificados",
                table: "Historico_Falta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
