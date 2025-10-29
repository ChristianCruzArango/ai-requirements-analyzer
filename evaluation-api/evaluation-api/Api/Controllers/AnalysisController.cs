using Microsoft.AspNetCore.Mvc;
using evaluation_api.Application.UseCases.Analysis;
using evaluation_api.Api.Common;
using evaluation_api.Domain.Exceptions;

namespace evaluation_api.Api.Controllers;

/// <summary>
/// Controlador para análisis de especificaciones de software con IA
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly AnalyzeSpecificationHandler _analyzeHandler;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        AnalyzeSpecificationHandler analyzeHandler,
        ILogger<AnalysisController> logger)
    {
        _analyzeHandler = analyzeHandler;
        _logger = logger;
    }

    /// <summary>
    /// Analiza una especificación de software y retorna procesos, subprocesos y casos de uso
    /// </summary>
    /// <param name="request">Especificación y tipo de análisis</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Análisis completo con procesos</returns>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(ApiResponse<AnalyzeSpecificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AnalyzeSpecificationResponse>>> AnalyzeSpecification(
        [FromBody] AnalyzeSpecificationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }

        _logger.LogInformation("📨 Nueva solicitud de análisis recibida");

        var response = await _analyzeHandler.Handle(request, cancellationToken);

        return Ok(ApiResponse<AnalyzeSpecificationResponse>.SuccessResponse(response));
    }
}
