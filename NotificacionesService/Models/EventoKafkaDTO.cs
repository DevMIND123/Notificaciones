using System.Text.Json.Serialization;

namespace NotificacionesService.Models;

public class EventoKafkaDTO
{
    [JsonPropertyName("id")]
    public int IdUsuario { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
}
