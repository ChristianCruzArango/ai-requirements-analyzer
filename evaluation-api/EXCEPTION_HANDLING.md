# Manejo de Excepciones y Respuestas Estándar

## 🎯 Filosofía: Sin Try-Catch en Controladores

Este proyecto utiliza un **middleware centralizado** para el manejo de excepciones. Los controladores **NO** usan `try-catch`, simplemente lanzan excepciones y el middleware las captura automáticamente.

### ✅ Ventajas

1. **Código más limpio** - Los controladores son más simples y legibles
2. **Respuestas consistentes** - Todas las respuestas tienen el mismo formato
3. **Mantenimiento fácil** - Cambiar el formato de error se hace en un solo lugar
4. **Separación de responsabilidades** - Los controladores se enfocan en la lógica de negocio

## 📦 Formato de Respuesta Estándar (ApiResponse)

Todas las respuestas de la API siguen este formato:

### Respuesta Exitosa

```json
{
  "success": true,
  "data": {
    // Los datos solicitados
  },
  "error": null,
  "timestamp": "2025-01-29T12:00:00Z"
}
```

### Respuesta de Error

```json
{
  "success": false,
  "data": null,
  "error": {
    "message": "Mensaje descriptivo del error",
    "code": "ERROR_CODE",
    "statusCode": 400,
    "validationErrors": {
      "Campo1": ["Error 1", "Error 2"],
      "Campo2": ["Error 3"]
    }
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
```

## 🚨 Tipos de Excepciones

### 1. ValidationException (400)

**Cuándo usarla:** Errores de validación de datos de entrada

```csharp
// Un solo campo
throw new ValidationException("Nombre", "El nombre es requerido");

// Múltiples campos
var errors = new Dictionary<string, string[]>
{
    { "Nombre", new[] { "El nombre es requerido", "Máximo 100 caracteres" } },
    { "Email", new[] { "Email inválido" } }
};
throw new ValidationException(errors);
```

**Respuesta:**
```json
{
  "success": false,
  "error": {
    "message": "Error en Nombre: El nombre es requerido",
    "code": "VALIDATION_ERROR",
    "statusCode": 400,
    "validationErrors": {
      "Nombre": ["El nombre es requerido"]
    }
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
```

### 2. NotFoundException (404)

**Cuándo usarla:** Recurso no encontrado

```csharp
throw new NotFoundException("Proceso", 123);
// o
throw new NotFoundException("No se encontró el recurso solicitado");
```

**Respuesta:**
```json
{
  "success": false,
  "error": {
    "message": "Proceso con ID 123 no encontrado",
    "code": "NOT_FOUND",
    "statusCode": 404
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
```

### 3. AIServiceException (502)

**Cuándo usarla:** Errores del servicio de IA (OpenRouter, OpenAI, etc.)

```csharp
throw new AIServiceException("Error al comunicarse con OpenRouter API");
// o con excepción interna
throw new AIServiceException("Error en análisis de IA", innerException);
```

**Respuesta:**
```json
{
  "success": false,
  "error": {
    "message": "Error al comunicarse con OpenRouter API",
    "code": "AI_SERVICE_ERROR",
    "statusCode": 502
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
```

### 4. DatabaseException (500)

**Cuándo usarla:** Errores de base de datos

```csharp
throw new DatabaseException("Error al guardar en PostgreSQL");
// o con excepción interna
throw new DatabaseException("Error de conexión", dbException);
```

**Respuesta:**
```json
{
  "success": false,
  "error": {
    "message": "Error al guardar en PostgreSQL",
    "code": "DATABASE_ERROR",
    "statusCode": 500
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
```

### 5. Excepciones No Controladas (500)

Cualquier excepción que no sea de las anteriores se captura como error interno:

**Respuesta:**
```json
{
  "success": false,
  "error": {
    "message": "Error interno del servidor",
    "code": "INTERNAL_SERVER_ERROR",
    "statusCode": 500
  },
  "timestamp": "2025-01-29T12:00:00Z"
}
```

## 🔧 Ejemplo de Uso en Controlador

### ❌ INCORRECTO (No hacer)

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Proceso>> GetById(int id)
{
    try  // ❌ NO usar try-catch
    {
        var proceso = await _repository.GetByIdAsync(id);

        if (proceso == null)
        {
            return NotFound(new { error = "No encontrado" });  // ❌ Formato inconsistente
        }

        return Ok(proceso);  // ❌ Sin ApiResponse
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);  // ❌ Formato inconsistente
    }
}
```

### ✅ CORRECTO

```csharp
[HttpGet("{id}")]
[ProducesResponseType(typeof(ApiResponse<Proceso>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
public async Task<ActionResult<ApiResponse<Proceso>>> GetById(int id)
{
    var proceso = await _repository.GetByIdAsync(id);

    if (proceso == null)
    {
        throw new NotFoundException("Proceso", id);  // ✅ Lanzar excepción
    }

    return Ok(ApiResponse<Proceso>.SuccessResponse(proceso));  // ✅ ApiResponse
}
```

## 🛡️ ExceptionHandlingMiddleware

El middleware captura todas las excepciones y las convierte en respuestas estándar:

```csharp
// En Program.cs (DEBE ser el PRIMERO)
app.UseExceptionHandling();  // ✅ Primero en el pipeline

app.UseSwagger();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

### Flujo de Manejo

```
1. Request llega al controlador
   ↓
2. Controlador ejecuta lógica
   ↓
3. Si hay error, lanza excepción (throw)
   ↓
4. Middleware captura la excepción
   ↓
5. Identifica el tipo de excepción
   ↓
6. Construye ApiResponse con error
   ↓
7. Retorna JSON con status code correcto
```

## 📝 Ventajas para el Frontend

El frontend Angular siempre recibe el mismo formato, facilitando el manejo:

```typescript
// Servicio Angular
analyzeSpecification(request: AnalysisRequest): Observable<ApiResponse<AnalysisResult>> {
  return this.http.post<ApiResponse<AnalysisResult>>('/api/analysis/analyze', request)
    .pipe(
      map(response => {
        if (!response.success) {
          // Manejar error de forma consistente
          throw new Error(response.error?.message || 'Error desconocido');
        }
        return response.data!;
      })
    );
}
```

## 🎨 Crear Excepciones Personalizadas

Para crear una nueva excepción personalizada:

1. **Heredar de `BaseException`:**

```csharp
public class PaymentException : BaseException
{
    public PaymentException(string message)
        : base(message, 402, "PAYMENT_REQUIRED")
    {
    }
}
```

2. **Agregar al middleware:**

```csharp
// En ExceptionHandlingMiddleware.cs
var response = exception switch
{
    ValidationException validationEx => ...,
    NotFoundException notFoundEx => ...,
    PaymentException paymentEx => ApiResponse<object>.ErrorResponse(
        paymentEx.Message,
        paymentEx.ErrorCode,
        paymentEx.StatusCode
    ),
    // ... resto
};
```

## 📊 Códigos de Error Estándar

| Código | Status | Descripción |
|--------|--------|-------------|
| `VALIDATION_ERROR` | 400 | Errores de validación de datos |
| `NOT_FOUND` | 404 | Recurso no encontrado |
| `DATABASE_ERROR` | 500 | Error en base de datos |
| `AI_SERVICE_ERROR` | 502 | Error en servicio de IA |
| `INTERNAL_SERVER_ERROR` | 500 | Error interno no controlado |

## 🚀 Mejores Prácticas

1. ✅ **Siempre usar ApiResponse** en las respuestas exitosas
2. ✅ **Lanzar excepciones específicas** en vez de genéricas
3. ✅ **No usar try-catch** en controladores
4. ✅ **Documentar con `[ProducesResponseType]`** los tipos de retorno
5. ✅ **Mensajes de error descriptivos** para el usuario final
6. ✅ **Logging de errores** automático en el middleware

---

**Resultado:** API consistente, fácil de integrar y mantener! 🎯
