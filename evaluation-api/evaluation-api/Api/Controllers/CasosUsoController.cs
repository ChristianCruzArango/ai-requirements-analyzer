using Microsoft.AspNetCore.Mvc;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;
using evaluation_api.Api.Common;
using evaluation_api.Domain.Exceptions;

namespace evaluation_api.Api.Controllers;

/// <summary>
/// Controlador para gestión de casos de uso
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CasosUsoController : ControllerBase
{
    private readonly ICasoUsoRepository _casoUsoRepository;
    private readonly ILogger<CasosUsoController> _logger;

    public CasosUsoController(
        ICasoUsoRepository casoUsoRepository,
        ILogger<CasosUsoController> logger)
    {
        _casoUsoRepository = casoUsoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un caso de uso por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CasoUso>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CasoUso>>> GetById(int id, CancellationToken cancellationToken)
    {
        var casoUso = await _casoUsoRepository.GetByIdAsync(id, cancellationToken);

        if (casoUso == null)
        {
            throw new NotFoundException("Caso de uso", id);
        }

        return Ok(ApiResponse<CasoUso>.SuccessResponse(casoUso));
    }

    /// <summary>
    /// Obtiene todos los casos de uso de un subproceso específico
    /// </summary>
    [HttpGet("subproceso/{subprocesoId}")]
    [ProducesResponseType(typeof(ApiResponse<List<CasoUso>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CasoUso>>>> GetBySubprocesoId(int subprocesoId, CancellationToken cancellationToken)
    {
        var casosUso = await _casoUsoRepository.GetBySubprocesoIdAsync(subprocesoId, cancellationToken);
        return Ok(ApiResponse<List<CasoUso>>.SuccessResponse(casosUso));
    }

    /// <summary>
    /// Crea un nuevo caso de uso
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CasoUso casoUso, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(casoUso.Nombre))
        {
            throw new ValidationException("Nombre", "El nombre del caso de uso es requerido");
        }

        if (casoUso.IdSubproceso <= 0)
        {
            throw new ValidationException("IdSubproceso", "El ID del subproceso es requerido");
        }

        var idCasoUso = await _casoUsoRepository.CreateAsync(casoUso, cancellationToken);

        _logger.LogInformation("Caso de uso creado: {Nombre} (ID: {Id})", casoUso.Nombre, idCasoUso);

        return CreatedAtAction(nameof(GetById), new { id = idCasoUso }, ApiResponse<int>.SuccessResponse(idCasoUso));
    }
}
