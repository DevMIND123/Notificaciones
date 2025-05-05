using System.Text.Json.Serialization;

namespace NotificacionesService.Models;

public class EventoKafkaDTO
{
    [JsonPropertyName("email")]
    public string EmailUsuario { get; set; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]

    public string Tipo { get; set; } = string.Empty; // CLIENTE o EMPRESA o otro
}
