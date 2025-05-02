public class Notificacion
{
    public int Id { get; set; }

    public string EmailUsuario { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public bool Leido { get; set; } = false;

    public string TipoUsuario { get; set; } = string.Empty;
}
