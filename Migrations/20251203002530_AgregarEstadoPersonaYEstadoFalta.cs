using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoPersonaYEstadoFalta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "estado_persona",
                table: "Persona",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "estado_falta",
                table: "Falta",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado_persona",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "estado_falta",
                table: "Falta");
        }
    }
}
