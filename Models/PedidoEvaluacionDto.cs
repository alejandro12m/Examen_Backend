namespace examen_backend2.Models;

public class PedidoEvaluacionDto
{
    public decimal MontoPedido { get; set; }
    public ClienteDto? Cliente { get; set; }
    public CreditoDto? Credito { get; set; }
    public decimal SaldoDisponible { get; set; }
    public bool TieneCreditoSuficiente { get; set; }
    public string Estado { get; set; } = "pendiente";
    public string? Mensaje { get; set; }
}

