using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificacionesService.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoUsuario",
                table: "notificaciones",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoUsuario",
                table: "notificaciones");
        }
    }
}
