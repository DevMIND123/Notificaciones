using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificacionesService.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_notificaciones",
                table: "notificaciones");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "notificaciones");

            migrationBuilder.RenameTable(
                name: "notificaciones",
                newName: "Notificaciones");

            migrationBuilder.AddColumn<string>(
                name: "EmailUsuario",
                table: "Notificaciones",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notificaciones",
                table: "Notificaciones",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Notificaciones",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "EmailUsuario",
                table: "Notificaciones");

            migrationBuilder.RenameTable(
                name: "Notificaciones",
                newName: "notificaciones");

            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "notificaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_notificaciones",
                table: "notificaciones",
                column: "Id");
        }
    }
}
