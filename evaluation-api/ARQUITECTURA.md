# Arquitectura del Proyecto - Evaluation IA API

## 📐 Clean Architecture

El proyecto sigue **Clean Architecture** con separación clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────┐
│                    API LAYER (HTTP)                      │
│  - Controllers (AnalysisController, ProcesosController) │
│  - Request/Response DTOs                                 │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│              APPLICATION LAYER                           │
│  - Use Cases (Handlers)                                  │
│  - Interfaces (Ports)                                    │
│  - Services (SpecificationAnalysisService)              │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│                 DOMAIN LAYER                             │
│  - Models (Proceso, Subproceso, CasoUso)               │
│  - Enums (TipoCasoUso, TipoAnalisis)                   │
│  - Business Rules                                        │
└──────────────────────────────────────────────────────────┘
                 ▲
┌────────────────┴────────────────────────────────────────┐
│            INFRASTRUCTURE LAYER                          │
│  - Adapters (OpenRouterAdapter, OpenAIAdapter)          │
│  - Persistence (EF Core, Repositories)                  │
│  - External Services                                     │
└──────────────────────────────────────────────────────────┘
```

## 🔌 Patrón Adapter para Servicios de IA

### Ventaja Principal
**Cambiar de proveedor de IA sin modificar el código de negocio**

### Implementación

```csharp
// Interfaz genérica
public interface IAIService
{
    Task<AIResponse> GenerateCompletionAsync(AIRequest request, ...);
}

// Adaptador OpenRouter
public class OpenRouterAdapter : IAIService { ... }

// Futuro: Adaptador OpenAI
public class OpenAIAdapter : IAIService { ... }
```

### Cambio de Proveedor

**Solo modificar una línea en `Program.cs`:**

```csharp
// OpenRouter
builder.Services.AddHttpClient<IAIService, OpenRouterAdapter>();

// OpenAI (futuro)
builder.Services.AddHttpClient<IAIService, OpenAIAdapter>();
```

## ⚙️ Configuración Externalizada

### Todo configurable desde `appsettings.json`:

1. **Conexión a Base de Datos**
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
   }
   ```

2. **Configuración de OpenRouter**
   ```json
   "OpenRouter": {
     "ApiKey": "...",
     "DefaultModel": "deepseek/deepseek-chat",
     "Temperature": 0.7
   }
   ```

3. **Prompts de IA** (¡Sin tocar código!)
   ```json
   "AIPrompts": {
     "BasePrompt": "Eres un analista...",
     "DetailedAnalysis": { ... },
     "ProcessesAnalysis": { ... }
   }
   ```

## 🗂️ Estructura de Carpetas Completa

```
evaluation-api/
├── Api/
│   └── Controllers/
│       ├── AnalysisController.cs      # POST /api/analysis/analyze
│       ├── ProcesosController.cs      # CRUD Procesos
│       ├── SubprocesosController.cs   # CRUD Subprocesos
│       └── CasosUsoController.cs      # CRUD Casos de Uso
│
├── Application/
│   ├── Interfaces/
│   │   ├── IAIService.cs              # Interfaz genérica IA
│   │   ├── ISpecificationAnalysisService.cs
│   │   ├── IProcesoRepository.cs
│   │   ├── ISubprocesoRepository.cs
│   │   └── ICasoUsoRepository.cs
│   │
│   ├── Services/
│   │   └── SpecificationAnalysisService.cs  # Orquestación análisis
│   │
│   └── UseCases/
│       └── Analysis/
│           ├── AnalyzeSpecificationRequest.cs
│           ├── AnalyzeSpecificationResponse.cs
│           └── AnalyzeSpecificationHandler.cs
│
├── Domain/
│   ├── Enums/
│   │   ├── TipoCasoUso.cs             # 1=FUNCIONAL, 2=NO_FUNCIONAL, 3=SISTEMA
│   │   └── TipoAnalisis.cs            # Detailed, Processes, Basic
│   │
│   └── Models/
│       ├── Proceso.cs
│       ├── Subproceso.cs
│       └── CasoUso.cs
│
└── Infrastructure/
    ├── AI/
    │   └── Adapters/
    │       └── OpenRouterAdapter.cs   # Implementación OpenRouter
    │
    └── Persistence/
        ├── AppDbContext.cs            # EF Core DbContext
        │
        ├── Entities/
        │   ├── ProcesoEntity.cs       # Tabla 'proceso'
        │   ├── SubprocesoEntity.cs    # Tabla 'subproceso'
        │   └── CasoUsoEntity.cs       # Tabla 'caso_uso'
        │
        ├── Mappers/
        │   ├── ProcesoMapper.cs       # Domain ↔ Entity
        │   ├── SubprocesoMapper.cs
        │   └── CasoUsoMapper.cs
        │
        └── Repositories/
            ├── ProcesoRepository.cs   # Implementa IProcesoRepository
            ├── SubprocesoRepository.cs
            └── CasoUsoRepository.cs
```

## 🔄 Flujo de Datos

### 1. Análisis de Especificación (POST /api/analysis/analyze)

```
1. Usuario envía especificación al frontend Angular
   ↓
2. Frontend llama POST /api/analysis/analyze
   ↓
3. AnalysisController recibe request
   ↓
4. AnalyzeSpecificationHandler orquesta:
   ↓
   4a. Llama SpecificationAnalysisService
       ↓
       - Lee prompts desde appsettings.json
       - Construye system prompt y user prompt
       - Llama IAIService (OpenRouterAdapter)
       ↓
   4b. OpenRouterAdapter hace HTTP POST a OpenRouter API
       ↓
       - Envía prompts al modelo de IA (ej. DeepSeek)
       - Recibe respuesta JSON
       ↓
   4c. ParseAIResponse convierte JSON → AnalysisResult
   ↓
5. Handler guarda en PostgreSQL:
   - ProcesoRepository.CreateAsync()
   - SubprocesoRepository.CreateAsync()
   - CasoUsoRepository.CreateAsync()
   ↓
6. Response enviado al frontend
```

### 2. Consulta de Procesos (GET /api/procesos)

```
1. Frontend Angular llama GET /api/procesos
   ↓
2. ProcesosController recibe request
   ↓
3. ProcesoRepository.GetAllAsync()
   - EF Core query a PostgreSQL
   - SELECT * FROM proceso
   ↓
4. ProcesoMapper.ToDomain() convierte Entity → Domain
   ↓
5. Response JSON al frontend
```

## 🎯 Principios SOLID Aplicados

### Single Responsibility (S)
- Cada clase tiene una sola razón para cambiar
- Controllers: solo HTTP
- Repositories: solo persistencia
- Services: solo lógica de negocio

### Open/Closed (O)
- Patrón Adapter permite extender (nuevo proveedor IA) sin modificar código existente

### Liskov Substitution (L)
- `OpenRouterAdapter` y `OpenAIAdapter` (futuro) son intercambiables vía `IAIService`

### Interface Segregation (I)
- Interfaces específicas: `IProcesoRepository`, `ISubprocesoRepository`, etc.
- No interfaces gigantes con métodos no usados

### Dependency Inversion (D)
- Dependencias inyectadas vía constructor
- Application depende de `IAIService` (abstracción), no de `OpenRouterAdapter` (concreción)

## 📊 Base de Datos PostgreSQL

### Relaciones (CASCADE DELETE)

```
proceso (1) ─────< (N) subproceso (1) ─────< (N) caso_uso

Eliminar proceso → elimina sus subprocesos → elimina casos de uso
```

### Migrations (Entity Framework Core)

```bash
# Crear migración
dotnet ef migrations add InitialCreate

# Aplicar a base de datos
dotnet ef database update
```

## 🛠️ Tecnologías Clave

| Capa | Tecnología | Propósito |
|------|------------|-----------|
| API | ASP.NET Core 9 | Framework web |
| ORM | Entity Framework Core 9 | Mapeo objeto-relacional |
| DB | PostgreSQL + Npgsql | Base de datos |
| IA | OpenRouter API | Acceso a modelos gratuitos |
| Docs | Swagger/OpenAPI | Documentación interactiva |
| DI | Built-in .NET DI | Inyección de dependencias |

## 🚀 Escalabilidad

### Horizontal
- Stateless API → múltiples instancias detrás de load balancer
- PostgreSQL connection pooling

### Vertical
- Async/await en todos los métodos I/O
- `AsNoTracking()` en queries de solo lectura

### Caching (futuro)
- Redis para cachear procesos frecuentes
- IDistributedCache en .NET

## 🔐 Seguridad (Recomendaciones)

- [ ] Usar variables de entorno para API Keys en producción
- [ ] Implementar autenticación JWT
- [ ] Rate limiting en endpoints
- [ ] Validación estricta de inputs
- [ ] HTTPS obligatorio
- [ ] Sanitización de prompts de IA

## 📈 Monitoreo (futuro)

- Application Insights / Serilog para logging
- Health checks (`/health` endpoint)
- Métricas de uso de tokens IA
- Query performance tracking

---

**Autor:** Evaluation IA Team
**Versión:** 1.0.0
**Última actualización:** 2025-01-29
