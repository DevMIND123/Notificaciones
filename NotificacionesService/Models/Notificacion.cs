using System.ComponentModel.DataAnnotations.Schema;

namespace NotificacionesService.Models;

[Table("notificaciones")]
public class Notificacion
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Leido { get; set; } = false;
    public string TipoUsuario { get; set; } = string.Empty;
}
