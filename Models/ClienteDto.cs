using System.Text.Json.Serialization;

namespace examen_backend2.Models;

public class ClienteDto
{
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("ci")]
    public string Ci { get; set; } = string.Empty;

    [JsonPropertyName("categoria")]
    public string? Categoria { get; set; }
}

