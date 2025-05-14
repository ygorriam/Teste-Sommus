using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Sommus.DengueApi.Models;
using Sommus.DengueApi.Services;
using System.Text.Json.Serialization;
using System.Linq;


namespace Sommus.DengueApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DengueController : ControllerBase
{
    private readonly IDengueService _service;
    private readonly ILogger<DengueController> _logger;

    public DengueController(
        IDengueService service,
        ILogger<DengueController> logger)
    {
        _service = service;
        _logger = logger;
    }


    [HttpGet("por-semana")]
    [ProducesResponseType(200, Type = typeof(DengueSemanaResponse))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPorSemana(
   [FromQuery, Range(1, 53)] int ew,
   [FromQuery, Range(2000, 2100)] int ey)
    {
        try
        {
            // Obtém os dados da semana solicitada
            var dados = await _service.ObterDadosPorSemanaAsync(ew, ey);

            // Se não encontrar dados, retorna 404 Not Found
            if (dados == null || !dados.Any())
            {
                return NotFound();
            }

            // Criando uma lista de respostas para cada entrada
            var resposta = dados.Select(dado => new
            {
                semanaEpidemiologica = dado.SE,
                casosEstimados = dado.casos_est,
                casosNotificados = dado.casos, // Ou outro valor que represente os casos notificados
                nivelAlerta = dado.nivel,
                corNivelAlerta = GetCorNivelAlerta(dado.nivel)
            }).ToList();

            // Retorna OK com a lista de respostas
            return Ok(resposta);
        }
        catch (Exception ex)
        {
            // Em caso de erro, retorna 500
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // Método para obter a cor de nível de alerta
    private string GetCorNivelAlerta(int nivelAlerta)
    {
        switch (nivelAlerta)
        {
            case 1: return "green";
            case 2: return "yellow";
            case 3: return "orange";
            case 4: return "red";
            default: return "gray";
        }
    }



    [HttpGet("ultimas-semanas")]
    [ProducesResponseType(200, Type = typeof(List<DengueSemanaResponse>))]
    [ProducesResponseType(400, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetUltimasSemanas(
        [FromQuery, Range(1, 52)] int quantidade = 3)
    {
        try
        {
            var dados = await _service.ObterUltimasSemanasAsync(quantidade);
            return Ok(dados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar últimas semanas");
            return BadRequest(new ErrorResponse
            {
                Message = "Erro ao processar requisição",
                Details = ex.Message
            });
        }
    }

    [HttpGet("semanas-com-alerta")]
    [ProducesResponseType(200, Type = typeof(List<DengueSemanaResponse>))]
    [ProducesResponseType(400, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetSemanasComAlerta(
        [FromQuery, Range(1, 52)] int quantidade = 3,
        [FromQuery] int nivelMinimo = 2)
    {
        try
        {
            var dados = await _service.ObterSemanasComAlertaAsync(quantidade, nivelMinimo);
            return Ok(dados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar semanas com alerta");
            return BadRequest(new ErrorResponse
            {
                Message = "Erro ao processar requisição",
                Details = ex.Message
            });
        }
    }
}

public record ErrorResponse
{
    public required string Message { get; init; }
    public required string Details { get; init; }
}