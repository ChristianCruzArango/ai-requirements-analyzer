# Evaluation IA API

API backend para análisis de especificaciones de software usando Inteligencia Artificial (OpenRouter) con arquitectura limpia en .NET 9.

## 🏗️ Arquitectura

Este proyecto sigue **Clean Architecture** con las siguientes capas:

```
evaluation-api/
├── Api/                          # Controladores HTTP
│   └── Controllers/
├── Application/                  # Lógica de negocio
│   ├── Interfaces/              # Contratos (puertos)
│   ├── Services/                # Servicios de aplicación
│   └── UseCases/                # Casos de uso (handlers)
├── Domain/                       # Núcleo del negocio
│   ├── Enums/
│   └── Models/
└── Infrastructure/               # Adaptadores
    ├── AI/
    │   └── Adapters/            # Adaptadores para servicios de IA
    └── Persistence/
        ├── Entities/
        ├── Mappers/
        └── Repositories/
```

## 🚀 Características

- ✅ **Patrón Adapter** para servicios de IA (fácil cambiar de OpenRouter a OpenAI)
- ✅ **Entity Framework Core** con PostgreSQL
- ✅ **Clean Architecture** (Domain, Application, Infrastructure, Api)
- ✅ **Manejo de excepciones centralizado** (sin try-catch en controladores)
- ✅ **ApiResponse estandarizado** para todas las respuestas
- ✅ **Prompts configurables** desde appsettings.json
- ✅ **Swagger** para documentación de API
- ✅ **Inyección de dependencias**
- ✅ **Logging** estructurado
- ✅ **CORS** configurado para Angular

## 📋 Prerrequisitos

1. **.NET 9 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **PostgreSQL 12+** - [Descargar](https://www.postgresql.org/download/)
3. **OpenRouter API Key** - [Obtener en OpenRouter](https://openrouter.ai/)

## ⚙️ Configuración

### 1. Configurar base de datos PostgreSQL

Crear la base de datos y tablas:

\`\`\`sql
CREATE DATABASE evaluacion_ia;

\\c evaluacion_ia;

CREATE TABLE proceso (
    id_proceso SERIAL PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT
);

CREATE TABLE subproceso (
    id_subproceso SERIAL PRIMARY KEY,
    id_proceso INTEGER NOT NULL REFERENCES proceso(id_proceso) ON DELETE CASCADE,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT
);

CREATE TABLE caso_uso (
    id_caso_uso SERIAL PRIMARY KEY,
    id_subproceso INTEGER NOT NULL REFERENCES subproceso(id_subproceso) ON DELETE CASCADE,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    actor_principal VARCHAR(100),
    tipo_caso_uso INTEGER NOT NULL,
    precondiciones TEXT,
    postcondiciones TEXT,
    criterios_de_aceptacion TEXT
);
\`\`\`

### 2. Configurar appsettings.json

Editar `appsettings.json` y agregar tus credenciales:

\`\`\`json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=evaluacion_ia;Username=TU_USUARIO;Password=TU_PASSWORD"
  },
  "OpenRouter": {
    "ApiKey": "TU_OPENROUTER_API_KEY",
    "DefaultModel": "deepseek/deepseek-chat"
  }
}
\`\`\`

## 🏃 Ejecutar el proyecto

1. **Restaurar dependencias:**
   \`\`\`bash
   cd evaluation-api
   dotnet restore
   \`\`\`

2. **Ejecutar la API:**
   \`\`\`bash
   dotnet run
   \`\`\`

3. **Acceder a Swagger:**
   - Abrir navegador en: `http://localhost:5000`
   - Swagger UI estará disponible en la raíz

## 📡 Endpoints

### 🤖 Análisis con IA

#### POST /api/analysis/analyze

Analiza una especificación de software usando IA y guarda en la base de datos.

**Request Body:**
\`\`\`json
{
  "especificacion": "Necesito un sistema de gestión de inventario para una tienda...",
  "tipoAnalisis": 0
}
\`\`\`

**Tipos de Análisis:**
- `0` = Detailed (procesos, subprocesos y casos de uso completos)
- `1` = Processes (solo procesos y subprocesos)
- `2` = Basic (solo procesos)

**Response:**
\`\`\`json
{
  "procesos": [...],
  "resumen": "Sistema completo de gestión...",
  "recomendaciones": ["Implementar validaciones...", "..."]
}
\`\`\`

---

### 📋 Procesos

#### GET /api/procesos
Obtiene todos los procesos guardados.

**Response:**
\`\`\`json
[
  {
    "idProceso": 1,
    "nombre": "Gestión de Inventario",
    "descripcion": "Proceso principal para gestión de productos",
    "subprocesos": []
  }
]
\`\`\`

#### GET /api/procesos/{id}
Obtiene un proceso específico con sus subprocesos.

**Response:**
\`\`\`json
{
  "idProceso": 1,
  "nombre": "Gestión de Inventario",
  "descripcion": "Proceso principal...",
  "subprocesos": [
    {
      "idSubproceso": 1,
      "idProceso": 1,
      "nombre": "Registrar Productos",
      "descripcion": "...",
      "casosUso": []
    }
  ]
}
\`\`\`

#### POST /api/procesos
Crea un nuevo proceso.

**Request Body:**
\`\`\`json
{
  "nombre": "Gestión de Usuarios",
  "descripcion": "Proceso para administrar usuarios del sistema"
}
\`\`\`

#### DELETE /api/procesos/{id}
Elimina un proceso y sus subprocesos relacionados (CASCADE).

---

### 📄 Subprocesos

#### GET /api/subprocesos/{id}
Obtiene un subproceso específico con sus casos de uso.

**Response:**
\`\`\`json
{
  "idSubproceso": 1,
  "idProceso": 1,
  "nombre": "Registrar Productos",
  "descripcion": "...",
  "casosUso": [
    {
      "idCasoUso": 1,
      "nombre": "Crear Producto",
      "tipoCasoUso": 1,
      "..."
    }
  ]
}
\`\`\`

#### GET /api/subprocesos/proceso/{procesoId}
Obtiene todos los subprocesos de un proceso específico con sus casos de uso.

#### POST /api/subprocesos
Crea un nuevo subproceso.

**Request Body:**
\`\`\`json
{
  "idProceso": 1,
  "nombre": "Actualizar Productos",
  "descripcion": "Permite modificar información de productos existentes"
}
\`\`\`

---

### ✔️ Casos de Uso

#### GET /api/casosuso/{id}
Obtiene un caso de uso específico.

**Response:**
\`\`\`json
{
  "idCasoUso": 1,
  "idSubproceso": 1,
  "nombre": "Crear Producto",
  "descripcion": "Permite al usuario registrar un nuevo producto",
  "actorPrincipal": "Administrador",
  "tipoCasoUso": 1,
  "precondiciones": "El usuario debe estar autenticado",
  "postcondiciones": "El producto queda registrado en el sistema",
  "criteriosDeAceptacion": "El sistema valida que todos los campos obligatorios estén completos"
}
\`\`\`

#### GET /api/casosuso/subproceso/{subprocesoId}
Obtiene todos los casos de uso de un subproceso específico.

#### POST /api/casosuso
Crea un nuevo caso de uso.

**Request Body:**
\`\`\`json
{
  "idSubproceso": 1,
  "nombre": "Eliminar Producto",
  "descripcion": "Permite eliminar un producto del inventario",
  "actorPrincipal": "Administrador",
  "tipoCasoUso": 1,
  "precondiciones": "El producto debe existir",
  "postcondiciones": "El producto es eliminado del sistema",
  "criteriosDeAceptacion": "El sistema solicita confirmación antes de eliminar"
}
\`\`\`

**Tipos de Caso de Uso:**
- `1` = FUNCIONAL
- `2` = NO_FUNCIONAL
- `3` = SISTEMA

## 🔄 Cambiar de proveedor de IA

El proyecto usa el **patrón Adapter** para facilitar el cambio de proveedor de IA. Para cambiar de OpenRouter a OpenAI:

1. **Crear nuevo adaptador** en `Infrastructure/AI/Adapters/OpenAIAdapter.cs`:

\`\`\`csharp
public class OpenAIAdapter : IAIService
{
    // Implementar la interfaz IAIService
    // usando la API de OpenAI en vez de OpenRouter
}
\`\`\`

2. **Actualizar** `Program.cs`:

\`\`\`csharp
// Cambiar de:
builder.Services.AddHttpClient<IAIService, OpenRouterAdapter>();

// A:
builder.Services.AddHttpClient<IAIService, OpenAIAdapter>();
\`\`\`

¡Listo! El resto del código no necesita cambios.

## 🛠️ Tecnologías utilizadas

- **.NET 9** - Framework
- **Entity Framework Core 9** - ORM
- **Npgsql** - Driver de PostgreSQL
- **Swagger/OpenAPI** - Documentación
- **OpenRouter AI** - Servicio de IA (DeepSeek, GPT-3.5, Llama, etc.)
- **Middleware personalizado** - Manejo de excepciones centralizado

## 📋 Formato de Respuesta

Todas las respuestas siguen el formato `ApiResponse<T>`:

**Éxito:**
\`\`\`json
{
  "success": true,
  "data": { /* datos */ },
  "error": null,
  "timestamp": "2025-01-29T12:00:00Z"
}
\`\`\`

**Error:**
\`\`\`json
{
  "success": false,
  "data": null,
  "error": {
    "message": "Descripción del error",
    "code": "ERROR_CODE",
    "statusCode": 400,
    "validationErrors": {
      "campo": ["error1", "error2"]
    }
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
\`\`\`

Ver [EXCEPTION_HANDLING.md](./EXCEPTION_HANDLING.md) para más detalles.

## 📝 Modelos de IA gratuitos disponibles

- DeepSeek V3.1 (Free)
- OpenAI GPT-3.5 Turbo (Free)
- Meta Llama 3.2 3B (Free)
- Qwen 2 7B (Free)
- Google Gemma 2 9B (Free)

## 🔒 Seguridad

- **NO** commitear el archivo `appsettings.json` con credenciales reales
- Usar **variables de entorno** en producción
- El API Key de OpenRouter debe mantenerse privado

## 📄 Licencia

Este proyecto es parte de una evaluación técnica.
