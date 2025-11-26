using System.Text.Json.Serialization;

namespace examen_backend2.Models;

public class CreditoDto
{
    [JsonPropertyName("codigo")]
    public int Codigo { get; set; }

    [JsonPropertyName("clienteCi")]
    public int ClienteCi { get; set; }

    [JsonPropertyName("limiteCredito")]
    public decimal LimiteCredito { get; set; }

    [JsonPropertyName("saldoUsado")]
    public decimal SaldoUsado { get; set; }
}

