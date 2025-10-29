using Microsoft.AspNetCore.Mvc;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;
using evaluation_api.Api.Common;
using evaluation_api.Domain.Exceptions;

namespace evaluation_api.Api.Controllers;

/// <summary>
/// Controlador para gestión de subprocesos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubprocesosController : ControllerBase
{
    private readonly ISubprocesoRepository _subprocesoRepository;
    private readonly ICasoUsoRepository _casoUsoRepository;
    private readonly ILogger<SubprocesosController> _logger;

    public SubprocesosController(
        ISubprocesoRepository subprocesoRepository,
        ICasoUsoRepository casoUsoRepository,
        ILogger<SubprocesosController> logger)
    {
        _subprocesoRepository = subprocesoRepository;
        _casoUsoRepository = casoUsoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un subproceso por ID con sus casos de uso
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Subproceso>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Subproceso>>> GetById(int id, CancellationToken cancellationToken)
    {
        var subproceso = await _subprocesoRepository.GetByIdAsync(id, cancellationToken);

        if (subproceso == null)
        {
            throw new NotFoundException("Subproceso", id);
        }

        // Cargar casos de uso del subproceso
        subproceso.CasosUso = await _casoUsoRepository.GetBySubprocesoIdAsync(id, cancellationToken);

        return Ok(ApiResponse<Subproceso>.SuccessResponse(subproceso));
    }

    /// <summary>
    /// Obtiene todos los subprocesos de un proceso específico
    /// </summary>
    [HttpGet("proceso/{procesoId}")]
    [ProducesResponseType(typeof(ApiResponse<List<Subproceso>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Subproceso>>>> GetByProcesoId(int procesoId, CancellationToken cancellationToken)
    {
        var subprocesos = await _subprocesoRepository.GetByProcesoIdAsync(procesoId, cancellationToken);

        // Cargar casos de uso para cada subproceso
        foreach (var subproceso in subprocesos)
        {
            subproceso.CasosUso = await _casoUsoRepository.GetBySubprocesoIdAsync(subproceso.IdSubproceso!.Value, cancellationToken);
        }

        return Ok(ApiResponse<List<Subproceso>>.SuccessResponse(subprocesos));
    }

    /// <summary>
    /// Crea un nuevo subproceso
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] Subproceso subproceso, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subproceso.Nombre))
        {
            throw new ValidationException("Nombre", "El nombre del subproceso es requerido");
        }

        if (subproceso.IdProceso <= 0)
        {
            throw new ValidationException("IdProceso", "El ID del proceso es requerido");
        }

        var idSubproceso = await _subprocesoRepository.CreateAsync(subproceso, cancellationToken);

        _logger.LogInformation("Subproceso creado: {Nombre} (ID: {Id})", subproceso.Nombre, idSubproceso);

        return CreatedAtAction(nameof(GetById), new { id = idSubproceso }, ApiResponse<int>.SuccessResponse(idSubproceso));
    }
}
