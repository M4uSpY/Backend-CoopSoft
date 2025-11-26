using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class updateHistoricoTrabajador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "accion",
                table: "Historico_Trabajador",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "apartados_modificados",
                table: "Historico_Trabajador",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accion",
                table: "Historico_Trabajador");

            migrationBuilder.DropColumn(
                name: "apartados_modificados",
                table: "Historico_Trabajador");
        }
    }
}
