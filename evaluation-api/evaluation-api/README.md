# Evaluation API - Backend .NET

API RESTful desarrollada en .NET 9 para análisis de especificaciones de software usando Inteligencia Artificial. Utiliza OpenRouter para procesamiento de IA y PostgreSQL para persistencia de datos.

**Video demostración**: [Ver en YouTube](https://youtu.be/V-VEs5XVrx8)

## 📋 Descripción

Backend que proporciona servicios de análisis de especificaciones de software mediante IA. Recibe descripciones de necesidades de software y genera automáticamente estructuras de procesos, subprocesos y casos de uso utilizando modelos de lenguaje como DeepSeek, GPT-3.5, Llama, etc.

**Funcionalidades Principales:**
- Análisis de especificaciones con IA (OpenRouter AI)
- CRUD completo de Procesos, Subprocesos y Casos de Uso
- Persistencia en base de datos PostgreSQL
- Arquitectura limpia (Clean Architecture)
- Patrón Adapter para servicios de IA
- Manejo centralizado de excepciones
- Respuestas estandarizadas con ApiResponse

## 🛠️ Tecnologías

- **.NET 9** - Framework backend
- **ASP.NET Core** - Web API
- **Entity Framework Core 9** - ORM
- **PostgreSQL** - Base de datos
- **OpenRouter AI** - Servicios de IA
- **Swagger/OpenAPI** - Documentación de API
- **C# 13** - Lenguaje de programación

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con 4 capas:

```
evaluation-api/
├── Api/                        # Capa de presentación
│   ├── Controllers/           # Endpoints REST
│   ├── Middleware/            # Middleware personalizado
│   └── Common/                # Respuestas estandarizadas
│
├── Application/                # Lógica de aplicación
│   ├── Interfaces/            # Contratos de servicios
│   ├── Services/              # Servicios de negocio
│   └── UseCases/              # Casos de uso
│
├── Domain/                     # Núcleo del negocio
│   ├── Models/                # Entidades del dominio
│   ├── Enums/                 # Enumeraciones
│   └── Exceptions/            # Excepciones personalizadas
│
└── Infrastructure/             # Adaptadores externos
    ├── Persistence/           # Acceso a datos (EF Core)
    │   ├── Entities/         # Entidades de BD
    │   ├── Repositories/     # Implementación de repositorios
    │   └── Mappers/          # Mapeo Domain ↔ DB
    └── AI/                    # Servicios de IA
        └── Adapters/         # Adaptadores (OpenRouter, etc.)
```

## 🚀 Instalación y Ejecución

### Requisitos Previos
- .NET 9 SDK
- PostgreSQL 14+
- OpenRouter API Key (o compatible)

### Pasos de Instalación

1. **Navegar al proyecto**
```bash
cd evaluation-api/evaluation-api
```

2. **Restaurar dependencias**
```bash
dotnet restore
```

3. **Configurar Base de Datos**

Crear base de datos en PostgreSQL:
```sql
CREATE DATABASE evaluacion_ia;
CREATE USER hallc_user WITH PASSWORD 'hallc_password';
GRANT ALL PRIVILEGES ON DATABASE evaluacion_ia TO hallc_user;
```

4. **Configurar appsettings.json**

Editar `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=evaluacion_ia;Username=hallc_user;Password=hallc_password"
  },
  "OpenRouter": {
    "ApiUrl": "https://openrouter.ai/api/v1",
    "ApiKey": "TU_API_KEY_AQUI",
    "DefaultModel": "deepseek/deepseek-chat",
    "Temperature": 0.7,
    "MaxTokens": 4000
  }
}
```

5. **Aplicar migraciones**
```bash
dotnet ef database update
```

6. **Ejecutar en desarrollo**
```bash
dotnet run
```

La API estará disponible en:
- **Swagger UI**: `http://localhost:5000`
- **API Base**: `http://localhost:5000/api`

7. **Compilar para producción**
```bash
dotnet publish -c Release -o ./publish
```

## 📡 Endpoints Principales

### Analysis Controller

#### POST /api/analysis/analyze
Analiza una especificación de software con IA

**Request:**
```json
{
  "especificacion": "Necesito un sistema de gestión de inventario...",
  "tipoAnalisis": "detailed"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "procesos": [...],
    "resumen": "...",
    "recomendaciones": [...]
  },
  "error": null,
  "timestamp": "2025-10-29T..."
}
```

### Procesos Controller

- **GET /api/procesos** - Obtener todos los procesos
- **GET /api/procesos/{id}** - Obtener proceso por ID
- **POST /api/procesos** - Crear nuevo proceso
- **PUT /api/procesos/{id}** - Actualizar proceso
- **DELETE /api/procesos/{id}** - Eliminar proceso

### Subprocesos Controller

- **GET /api/subprocesos** - Obtener todos los subprocesos
- **GET /api/subprocesos/{id}** - Obtener subproceso por ID
- **GET /api/subprocesos/proceso/{procesoId}** - Obtener por proceso
- **POST /api/subprocesos** - Crear subproceso
- **DELETE /api/subprocesos/{id}** - Eliminar subproceso

### CasosUso Controller

- **GET /api/casosuso** - Obtener todos los casos de uso
- **GET /api/casosuso/{id}** - Obtener caso de uso por ID
- **GET /api/casosuso/subproceso/{subprocesoId}** - Obtener por subproceso
- **POST /api/casosuso** - Crear caso de uso
- **DELETE /api/casosuso/{id}** - Eliminar caso de uso

## 🔧 Configuración

### OpenRouter AI

El proyecto usa OpenRouter para acceso a múltiples modelos de IA:

**Modelos gratuitos disponibles:**
- `deepseek/deepseek-chat` (Recomendado)
- `openai/gpt-3.5-turbo`
- `meta-llama/llama-3.2-3b-instruct:free`
- `qwen/qwen-2-7b-instruct:free`
- `google/gemma-2-9b-it:free`

Para obtener una API Key:
1. Visita https://openrouter.ai/
2. Crea una cuenta
3. Obtén tu API Key
4. Configúrala en `appsettings.json`

### Prompts de IA

Todos los prompts están configurables en `appsettings.json` bajo la sección `AIPrompts`. Puedes personalizar:
- Prompt base del sistema
- Instrucciones por tipo de análisis
- Estructura JSON esperada
- Notas adicionales

### CORS

Configurado para permitir cualquier origen en desarrollo:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

## 📦 Paquetes NuGet

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

## 🎯 Patrón Adapter para IA

El proyecto implementa el patrón Adapter para servicios de IA, permitiendo cambiar fácilmente entre proveedores:

**Interfaz:**
```csharp
public interface IAIService
{
    Task<AIResponse> GenerateCompletionAsync(AIRequest request, CancellationToken cancellationToken);
}
```

**Implementaciones:**
- `OpenRouterAdapter` - OpenRouter AI (actual)
- Fácil agregar: `OpenAIAdapter`, `AnthropicAdapter`, etc.

**Cambiar proveedor:**
```csharp
// En Program.cs, cambiar:
builder.Services.AddHttpClient<IAIService, OpenRouterAdapter>();
// Por:
builder.Services.AddHttpClient<IAIService, NuevoAdapter>();
```

## 🛡️ Manejo de Errores

### Excepciones Personalizadas

```csharp
BaseException                  # Base para todas las excepciones
├── ValidationException       # Errores de validación (400)
├── NotFoundException         # Recurso no encontrado (404)
├── AIServiceException        # Error en servicio de IA (503)
└── DatabaseException         # Error de base de datos (500)
```

### Middleware de Excepciones

Manejo centralizado sin try-catch en controladores:

```csharp
app.UseExceptionHandling();
```

Convierte automáticamente excepciones a respuestas ApiResponse estandarizadas.

### Formato de Respuesta

Todas las respuestas siguen el formato `ApiResponse<T>`:

**Éxito:**
```json
{
  "success": true,
  "data": {...},
  "error": null,
  "timestamp": "2025-10-29T..."
}
```

**Error:**
```json
{
  "success": false,
  "data": null,
  "error": {
    "message": "Mensaje de error",
    "code": "ERROR_CODE",
    "statusCode": 400,
    "validationErrors": {...}
  },
  "timestamp": "2025-10-29T..."
}
```

## 🗄️ Base de Datos

### Modelo de Datos

```
Proceso (1)
  ↓
Subproceso (N)
  ↓
CasoUso (N)
```

**Tablas:**
- `Procesos` - Procesos principales
- `Subprocesos` - Subprocesos de cada proceso
- `CasosUso` - Casos de uso de cada subproceso

### Entity Framework Core

**Crear migración:**
```bash
dotnet ef migrations add NombreMigracion
```

**Aplicar migración:**
```bash
dotnet ef database update
```

**Revertir migración:**
```bash
dotnet ef database update MigracionAnterior
```

## 🔍 Swagger / OpenAPI

Swagger UI disponible en `http://localhost:5000`

Documentación automática de todos los endpoints con:
- Descripción de parámetros
- Modelos de request/response
- Códigos de estado HTTP
- Ejemplos de uso

## 🧪 Testing

```bash
# Ejecutar tests
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true
```

## 🐛 Troubleshooting

### Error de conexión a PostgreSQL
- Verificar que PostgreSQL esté corriendo
- Verificar credenciales en `appsettings.json`
- Verificar puerto (default: 5432)

### Error de OpenRouter API
- Verificar API Key en `appsettings.json`
- Verificar conexión a internet
- Revisar logs en consola para detalles

### CORS Error desde frontend
- Verificar que CORS esté habilitado
- Verificar orden de middleware (CORS debe ir primero)

### Puerto 5000 ocupado
Editar `Properties/launchSettings.json`:
```json
{
  "applicationUrl": "http://localhost:OTRO_PUERTO"
}
```

## 📝 Convenciones de Código

- **Nombres**: PascalCase para clases/métodos, camelCase para variables
- **Async**: Todos los métodos de I/O son async
- **Comentarios**: Solo XML summaries (///) para documentación
- **Sin try-catch**: Usar excepciones personalizadas
- **Inyección de dependencias**: Constructor injection
- **Interfaces**: Prefijo `I`

## 👨‍💻 Desarrollo

### Agregar nuevo endpoint

1. Crear caso de uso en `Application/UseCases`
2. Registrar en `Program.cs`
3. Crear método en controlador en `Api/Controllers`
4. Documentar con XML comments

### Agregar nuevo modelo de IA

1. Crear adaptador en `Infrastructure/AI/Adapters`
2. Implementar interfaz `IAIService`
3. Registrar en `Program.cs`

## 📄 Licencia

Proyecto de evaluación técnica - Uso educativo

## 🔗 Integración

Este backend se integra con:
- **Frontend Angular** (`evaluation-ia`) - Ver README del frontend
- **OpenRouter AI** - Servicio de IA externo
- **PostgreSQL** - Base de datos

## 📚 Recursos

- [.NET 9 Docs](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [OpenRouter AI](https://openrouter.ai/docs)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
