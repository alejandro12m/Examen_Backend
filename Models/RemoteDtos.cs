namespace Examen_Backend.Models;

public sealed class ClienteDto
{
    public string? Nombre { get; set; }
    public string? Ci { get; set; }
    public string? Categoria { get; set; }
}

public sealed class CreditoDto
{
    public int Codigo { get; set; }
    public int ClienteCi { get; set; }
    public decimal LimiteCredito { get; set; }
    public decimal SaldoUsado { get; set; }
}

public sealed class PedidoEvaluacionDto
{
    public ClienteDto? Cliente { get; set; }
    public CreditoDto? Credito { get; set; }
    public decimal MontoPedido { get; set; }
    public decimal SaldoDisponible { get; set; }
    public bool TieneCreditoSuficiente { get; set; }
    public string Estado { get; set; } = "desconocido";
    public string? Mensaje { get; set; }
}

