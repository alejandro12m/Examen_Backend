using System.Net.Http.Json;
using examen_backend2.Models;
using Microsoft.AspNetCore.Mvc;

namespace examen_backend2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private const string ClientesEndpoint = "https://programacionweb2examen3-production.up.railway.app/api/Clientes/Listar";
    private const string CreditosEndpoint = "https://programacionweb2examen3-production.up.railway.app/api/Creditos/Listar";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IHttpClientFactory httpClientFactory, ILogger<ReportController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("combined/{clienteCi}")]
    public async Task<IActionResult> GetCombinedReport(string clienteCi, [FromQuery] decimal montoPedido, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clienteCi))
        {
            return BadRequest(new { mensaje = "El parámetro clienteCi es obligatorio." });
        }

        if (montoPedido <= 0)
        {
            return BadRequest(new { mensaje = "El monto del pedido debe ser mayor a cero." });
        }

        var clienteCiNormalizado = clienteCi.Trim();
        if (!int.TryParse(clienteCiNormalizado, out var clienteCiNumerico))
        {
            return BadRequest(new { mensaje = "El CI del cliente debe ser numérico para evaluar su crédito." });
        }

        var httpClient = _httpClientFactory.CreateClient("RemoteApi");
        var erroresServicio = new List<string>();
        List<ClienteDto>? clientes = null;
        List<CreditoDto>? creditos = null;

        try
        {
            using var response = await httpClient.GetAsync(ClientesEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                erroresServicio.Add($"Clientes respondió {(int)response.StatusCode} ({response.StatusCode}).");
            }
            else
            {
                clientes = await response.Content.ReadFromJsonAsync<List<ClienteDto>>(cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            erroresServicio.Add("Clientes no respondió a tiempo.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al consultar el servicio de clientes.");
            erroresServicio.Add("No fue posible contactar al servicio de clientes.");
        }

        try
        {
            using var response = await httpClient.GetAsync(CreditosEndpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                erroresServicio.Add($"Créditos respondió {(int)response.StatusCode} ({response.StatusCode}).");
            }
            else
            {
                creditos = await response.Content.ReadFromJsonAsync<List<CreditoDto>>(cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            erroresServicio.Add("Créditos no respondió a tiempo.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al consultar el servicio de créditos.");
            erroresServicio.Add("No fue posible contactar al servicio de créditos.");
        }

        var evaluacion = new PedidoEvaluacionDto
        {
            MontoPedido = montoPedido
        };

        if (clientes is not null)
        {
            evaluacion.Cliente = clientes.FirstOrDefault(c => string.Equals(c.Ci, clienteCiNormalizado, StringComparison.OrdinalIgnoreCase));
        }

        if (creditos is not null)
        {
            evaluacion.Credito = creditos.FirstOrDefault(c => c.ClienteCi == clienteCiNumerico);
        }

        if (evaluacion.Credito is not null)
        {
            evaluacion.SaldoDisponible = Math.Max(0, evaluacion.Credito.LimiteCredito - evaluacion.Credito.SaldoUsado);
            evaluacion.TieneCreditoSuficiente = evaluacion.SaldoDisponible >= montoPedido;
        }

        if (erroresServicio.Count > 0)
        {
            evaluacion.Estado = "parcial";
            evaluacion.Mensaje = string.Join(" | ", erroresServicio.Distinct());
            return StatusCode(StatusCodes.Status503ServiceUnavailable, evaluacion);
        }

        if (evaluacion.Cliente is null)
        {
            return NotFound(new
            {
                mensaje = $"No se encontró información del cliente {clienteCiNormalizado}.",
                estado = "completo"
            });
        }

        if (evaluacion.Credito is null)
        {
            evaluacion.Estado = "completo";
            evaluacion.Mensaje = $"El cliente {clienteCiNormalizado} no tiene crédito registrado.";
            evaluacion.TieneCreditoSuficiente = false;
            evaluacion.SaldoDisponible = 0;
            return Ok(evaluacion);
        }

        evaluacion.Estado = "completo";
        evaluacion.Mensaje = evaluacion.TieneCreditoSuficiente
            ? "Pedido aprobado: el crédito disponible cubre el monto solicitado."
            : "Pedido rechazado: saldo insuficiente para cubrir el monto solicitado.";

        return Ok(evaluacion);
    }
}

