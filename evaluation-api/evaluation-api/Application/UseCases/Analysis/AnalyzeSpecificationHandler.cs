using evaluation_api.Application.Interfaces;
using evaluation_api.Domain.Models;

namespace evaluation_api.Application.UseCases.Analysis;

/// <summary>
/// Handler para analizar especificación de software usando IA y guardar en PostgreSQL
/// </summary>
public class AnalyzeSpecificationHandler
{
    private readonly ISpecificationAnalysisService _analysisService;
    private readonly IProcesoRepository _procesoRepository;
    private readonly ISubprocesoRepository _subprocesoRepository;
    private readonly ICasoUsoRepository _casoUsoRepository;
    private readonly ILogger<AnalyzeSpecificationHandler> _logger;

    public AnalyzeSpecificationHandler(
        ISpecificationAnalysisService analysisService,
        IProcesoRepository procesoRepository,
        ISubprocesoRepository subprocesoRepository,
        ICasoUsoRepository casoUsoRepository,
        ILogger<AnalyzeSpecificationHandler> logger)
    {
        _analysisService = analysisService;
        _procesoRepository = procesoRepository;
        _subprocesoRepository = subprocesoRepository;
        _casoUsoRepository = casoUsoRepository;
        _logger = logger;
    }

    public async Task<AnalyzeSpecificationResponse> Handle(
        AnalyzeSpecificationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🤖 Iniciando análisis de especificación con IA...");

        // 1. Obtener análisis de IA
        var analysisResult = await _analysisService.AnalyzeSpecificationAsync(
            request.Especificacion,
            request.TipoAnalisis,
            cancellationToken);

        _logger.LogInformation("✅ Análisis de IA completado. Procesos encontrados: {Count}", analysisResult.Procesos.Count);

        // 2. Guardar procesos, subprocesos y casos de uso en la base de datos
        foreach (var proceso in analysisResult.Procesos)
        {
            // Guardar proceso
            var idProceso = await _procesoRepository.CreateAsync(
                new Proceso(proceso.Nombre, proceso.Descripcion),
                cancellationToken);

            _logger.LogInformation("📝 Proceso guardado: {Nombre} (ID: {Id})", proceso.Nombre, idProceso);

            // Guardar subprocesos
            if (proceso.Subprocesos != null)
            {
                foreach (var subproceso in proceso.Subprocesos)
                {
                    var idSubproceso = await _subprocesoRepository.CreateAsync(
                        new Subproceso(idProceso, subproceso.Nombre, subproceso.Descripcion),
                        cancellationToken);

                    _logger.LogInformation("  📄 Subproceso guardado: {Nombre} (ID: {Id})", subproceso.Nombre, idSubproceso);

                    // Guardar casos de uso
                    if (subproceso.CasosUso != null)
                    {
                        foreach (var casoUso in subproceso.CasosUso)
                        {
                            var idCasoUso = await _casoUsoRepository.CreateAsync(
                                new CasoUso(
                                    idSubproceso,
                                    casoUso.Nombre,
                                    casoUso.TipoCasoUso,
                                    casoUso.Descripcion,
                                    casoUso.ActorPrincipal,
                                    casoUso.Precondiciones,
                                    casoUso.Postcondiciones,
                                    casoUso.CriteriosDeAceptacion),
                                cancellationToken);

                            _logger.LogInformation("    ✔ Caso de uso guardado: {Nombre} (ID: {Id})", casoUso.Nombre, idCasoUso);
                        }
                    }
                }
            }
        }

        _logger.LogInformation("🎉 Análisis completado y guardado en base de datos");

        return new AnalyzeSpecificationResponse(
            analysisResult.Procesos,
            analysisResult.Resumen,
            analysisResult.Recomendaciones);
    }
}
