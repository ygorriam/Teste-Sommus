using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sommus.DengueApi.Services;

namespace Sommus.DengueApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportacaoController : ControllerBase
{
    private readonly IDengueService _service;
    private readonly ILogger<ImportacaoController> _logger;

    public ImportacaoController(
        IDengueService service,
        ILogger<ImportacaoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Importar()
    {
        var (success, message) = await _service.ImportarDadosUltimos6MesesAsync();

        if (success)
        {
            _logger.LogInformation(message);
            return Ok(new
            {
                success = true,
                message = message
            });
        }
        else
        {
            _logger.LogError(message);
            return BadRequest(new
            {
                success = false,
                message = message
            });
        }
    }
}