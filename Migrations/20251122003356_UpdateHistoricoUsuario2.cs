using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHistoricoUsuario2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historico_Usuario_Usuario_id_usuario",
                table: "Historico_Usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Historico_Usuario_Usuario_id_usuario",
                table: "Historico_Usuario",
                column: "id_usuario",
                principalTable: "Usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historico_Usuario_Usuario_id_usuario",
                table: "Historico_Usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Historico_Usuario_Usuario_id_usuario",
                table: "Historico_Usuario",
                column: "id_usuario",
                principalTable: "Usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
