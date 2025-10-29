using Microsoft.AspNetCore.Mvc;
using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;
using evaluation_api.Api.Common;
using evaluation_api.Domain.Exceptions;

namespace evaluation_api.Api.Controllers;

/// <summary>
/// Controlador para gestión de procesos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProcesosController : ControllerBase
{
    private readonly IProcesoRepository _procesoRepository;
    private readonly ISubprocesoRepository _subprocesoRepository;
    private readonly ILogger<ProcesosController> _logger;

    public ProcesosController(
        IProcesoRepository procesoRepository,
        ISubprocesoRepository subprocesoRepository,
        ILogger<ProcesosController> logger)
    {
        _procesoRepository = procesoRepository;
        _subprocesoRepository = subprocesoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los procesos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Proceso>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<Proceso>>>> GetAll(CancellationToken cancellationToken)
    {
        var procesos = await _procesoRepository.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<Proceso>>.SuccessResponse(procesos));
    }

    /// <summary>
    /// Obtiene un proceso por ID con sus subprocesos
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Proceso>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Proceso>>> GetById(int id, CancellationToken cancellationToken)
    {
        var proceso = await _procesoRepository.GetByIdAsync(id, cancellationToken);

        if (proceso == null)
        {
            throw new NotFoundException("Proceso", id);
        }

        // Cargar subprocesos del proceso
        proceso.Subprocesos = await _subprocesoRepository.GetByProcesoIdAsync(id, cancellationToken);

        return Ok(ApiResponse<Proceso>.SuccessResponse(proceso));
    }

    /// <summary>
    /// Crea un nuevo proceso
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] Proceso proceso, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(proceso.Nombre))
        {
            throw new ValidationException("Nombre", "El nombre del proceso es requerido");
        }

        var idProceso = await _procesoRepository.CreateAsync(proceso, cancellationToken);

        _logger.LogInformation("Proceso creado: {Nombre} (ID: {Id})", proceso.Nombre, idProceso);

        return CreatedAtAction(nameof(GetById), new { id = idProceso }, ApiResponse<int>.SuccessResponse(idProceso));
    }

    /// <summary>
    /// Elimina un proceso y sus subprocesos relacionados
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        var proceso = await _procesoRepository.GetByIdAsync(id, cancellationToken);

        if (proceso == null)
        {
            throw new NotFoundException("Proceso", id);
        }

        await _procesoRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Proceso eliminado: ID {Id}", id);

        return NoContent();
    }
}
