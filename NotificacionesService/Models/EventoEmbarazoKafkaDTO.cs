using System.Text.Json.Serialization;

namespace NotificacionesService.Models
{
    public class EventoEmbarazoKafkaDTO
    {
        [JsonPropertyName("emailUsuario")]
        public string EmailUsuario { get; set; } = string.Empty;

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
