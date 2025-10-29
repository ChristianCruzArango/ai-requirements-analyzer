
# .NET Backend Best Practices (Clean, Escalable y Fácil de Mantener)

> Opinadas para APIs tipo ASP.NET Core (.NET 9+), arquitectura limpia, Dapper o EF Core, y equipos que quieren crecer sin dolor.

---

## 1. Estructura de carpetas

Mantén cada capa separada por intención, no por tipo técnico. Ejemplo (API monolítica limpia):

```text
src/
 ├─ Api/                          # Capa de presentación (controllers, middlewares, DTOs)
 │   ├─ Controllers/
 │   ├─ Dtos/
 │   ├─ Middleware/
 │   └─ Program.cs
 │
 ├─ Application/                  # Lógica de negocio orquestada (casos de uso / services)
 │   ├─ UseCases/
 │   │    ├─ Users/
 │   │    │    ├─ CreateUser/
 │   │    │    │    ├─ CreateUserRequest.cs
 │   │    │    │    ├─ CreateUserResponse.cs
 │   │    │    │    └─ CreateUserHandler.cs
 │   │    └─ ...
 │   ├─ Interfaces/              # Puertos (contratos) que la app necesita
 │   └─ Common/
 │        ├─ Results/
 │        └─ Validators/
 │
 ├─ Domain/                       # Núcleo puro: reglas de negocio puras
 │   ├─ Models/
 │   │    ├─ User.cs
 │   │    ├─ Role.cs
 │   │    └─ ...
 │   ├─ Enums/
 │   ├─ ValueObjects/
 │   ├─ Exceptions/
 │   └─ Services/                # Domain services sin infraestructura
 │
 ├─ Infrastructure/               # Adaptadores concretos
 │   ├─ Persistence/
 │   │    ├─ Entities/           # Tablas/Bases de datos (ORM/Dapper)
 │   │    │    ├─ UserEntity.cs
 │   │    │    └─ RoleEntity.cs
 │   │    ├─ Mappers/
 │   │    │    ├─ UserMapper.cs
 │   │    └─ Repositories/
 │   │         ├─ UserRepository.cs
 │   │         └─ RoleRepository.cs
 │   ├─ Messaging/
 │   ├─ Email/
 │   ├─ WhatsApp/
 │   └─ Storage/
 │
 └─ Tests/
     ├─ Api.Tests/
     ├─ Application.Tests/
     ├─ Domain.Tests/
     └─ Infrastructure.Tests/
```

### Reglas:
- **Api** no habla directo con la base de datos.
- **Application** no sabe _cómo_ se guarda, solo sabe que existe `IUserRepository`.
- **Domain** NO referencia a ninguna otra capa.
- **Infrastructure** implementa los repositorios (`UserRepository : IUserRepository`) y se registra en `Program.cs`.

---

## 2. Nombres de carpetas y archivos

### Modelos / Entidades de Dominio
- Carpeta: `Domain/Models/`
- Archivo: `User.cs`
- Clase: `public class User`

### Entidades de Persistencia (tablas/BBDD)
- Carpeta: `Infrastructure/Persistence/Entities/`
- Archivo: `UserEntity.cs`
- Clase: `public class UserEntity`

> Regla: `User` = concepto de negocio.
> `UserEntity` = cómo lo guardas en la base de datos.

### Controladores
- Carpeta: `Api/Controllers/`
- Archivo: `UsersController.cs`
- Clase: `public class UsersController : ControllerBase`

### Casos de uso
- Carpeta: `Application/UseCases/Users/CreateUser/`
- Archivo handler: `CreateUserHandler.cs`
- Clase: `public class CreateUserHandler`

---

## 3. Convenciones de nombres

### Variables
- `camelCase` para variables locales / parámetros.
- `PascalCase` para propiedades públicas y clases.
- Evita nombres cortos tipo `a`, `tmp`, `x`. Prefiere claridad:
  - ❌ `var usr = repo.Get(id);`
  - ✅ `var existingUser = _userRepository.GetById(userId);`

### Campos privados
- Usa `_` + camelCase.
  ```csharp
  private readonly IUserRepository _userRepository;
  ```

### Interfaces
- Empiezan con `I`.
  ```csharp
  public interface IUserRepository
  ```

### DTOs / Requests / Responses
- Sufijo `Request`, `Response`, `Dto`.
  - `CreateUserRequest`
  - `CreateUserResponse`
  - `UserDto`

---

## 4. Controlador limpio (Controller)

Objetivo: El controlador **NO** debe tener lógica de negocio. Solo:
1. Validar entrada.
2. Llamar caso de uso.
3. Mapear respuesta HTTP.

**Archivo:** `Api/Controllers/UsersController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.UseCases.Users.CreateUser;

namespace MyApp.Api.Controllers;

/// <summary>
/// Endpoints relacionados con usuarios (crear, listar, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly CreateUserHandler _createUserHandler;

    public UsersController(CreateUserHandler createUserHandler)
    {
        _createUserHandler = createUserHandler;
    }

    /// <summary>
    /// Crea un nuevo usuario y devuelve sus datos con token de sesión.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateUserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _createUserHandler.Handle(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Errors);

        return Ok(result.Data);
    }
}
```

### Buenas prácticas aplicadas:
- `[ApiController]` + `ModelState` validation automática.
- Inyección de casos de uso (no de repositorios, no de `DbContext`).
- Métodos `async`.
- Respuestas claras `ActionResult<T>`.

---

## 5. Caso de uso (Use Case / Handler)

Este es el corazón de la app. Aquí va la lógica para crear usuario: validaciones de negocio, interacción con repositorio, etc.  
**NO** hagas esa lógica en el Controller.

**Archivo:** `Application/UseCases/Users/CreateUser/CreateUserHandler.cs`

```csharp
using MyApp.Application.Interfaces;
using MyApp.Domain.Models;

namespace MyApp.Application.UseCases.Users.CreateUser;

/// <summary>
/// Orquesta la creación de un usuario:
/// 1. Validar reglas de negocio (email único, etc.)
/// 2. Guardar en el repositorio
/// 3. Retornar DTO listo para API
/// </summary>
public class CreateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public CreateUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<CreateUserResponse>> Handle(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Validaciones de negocio
        var alreadyExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (alreadyExists)
        {
            return Result<CreateUserResponse>.Fail("EMAIL_ALREADY_USED");
        }

        // 2. Construir entidad de dominio (no la entity de BD aún)
        var user = new User(
            id: Guid.NewGuid(),
            fullName: request.FullName,
            email: request.Email,
            passwordHash: _passwordHasher.Hash(request.Password),
            isActive: true
        );

        // 3. Guardar en BD vía repositorio
        await _userRepository.CreateAsync(user, cancellationToken);

        // 4. Generar token (ej. login inmediato post-create)
        var token = _jwtTokenService.GenerateToken(user);

        // 5. Preparar respuesta limpia para el Controller
        var response = new CreateUserResponse(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            Token: token
        );

        return Result<CreateUserResponse>.Ok(response);
    }
}
```

### Buenas prácticas aplicadas:
- `Handler` recibe servicios por constructor (`IUserRepository`, `IJwtTokenService`, etc.).
- No depende de ASP.NET Core (`ControllerBase`, `HttpContext`, etc.).
- Devuelve siempre un `Result<T>` predecible.

---

## 6. DTOs claros (Requests / Responses)

**Archivo:** `Application/UseCases/Users/CreateUser/CreateUserRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Datos necesarios para crear usuario desde la API pública.
/// </summary>
public class CreateUserRequest
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
```

**Archivo:** `Application/UseCases/Users/CreateUser/CreateUserResponse.cs`

```csharp
/// <summary>
/// Respuesta que recibe el frontend al crear el usuario.
/// </summary>
public record CreateUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Token
);
```

### Buenas prácticas aplicadas:
- Usa `record` para respuestas inmutables.
- Validaciones con `[Required]`, `[MaxLength]`, etc.
- Nada de lógica dentro de los DTOs.

---

## 7. Entidades de Dominio vs Entidades de BD

**Dominio (reglas de negocio):**  
`Domain/Models/User.cs`

```csharp
namespace MyApp.Domain.Models;

/// <summary>
/// Representa un usuario dentro del dominio.
/// NO conoce de base de datos ni de infraestructura.
/// </summary>
public class User
{
    public Guid Id { get; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }

    public User(Guid id, string fullName, string email, string passwordHash, bool isActive)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
```

**Infraestructura (tabla / persistencia):**  
`Infrastructure/Persistence/Entities/UserEntity.cs`

```csharp
namespace MyApp.Infrastructure.Persistence.Entities;

/// <summary>
/// Representa la fila en la tabla Users de la base de datos.
/// Puede tener anotaciones específicas de ORM o nombres de columna.
/// </summary>
public class UserEntity
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

**Mapper:**  
`Infrastructure/Persistence/Mappers/UserMapper.cs`

```csharp
using MyApp.Domain.Models;
using MyApp.Infrastructure.Persistence.Entities;

namespace MyApp.Infrastructure.Persistence.Mappers;

/// <summary>
/// Traduce entre el modelo del dominio y la entidad de base de datos.
/// Mantén esta lógica en un solo lugar.
/// </summary>
public static class UserMapper
{
    public static UserEntity ToEntity(User domain)
    {
        return new UserEntity
        {
            Id = domain.Id,
            FullName = domain.FullName,
            Email = domain.Email,
            PasswordHash = domain.PasswordHash,
            IsActive = domain.IsActive
        };
    }

    public static User ToDomain(UserEntity entity)
    {
        return new User(
            id: entity.Id,
            fullName: entity.FullName,
            email: entity.Email,
            passwordHash: entity.PasswordHash,
            isActive: entity.IsActive
        );
    }
}
```

### Buenas prácticas aplicadas:
- El `Mapper` es el único lugar que conoce ambos mundos.
- El dominio no referencia `Infrastructure`.
- Infraestructura sí puede referenciar dominio.

---

## 8. Repositorios

### Interfaz (puerto) en Application
**Archivo:** `Application/Interfaces/IUserRepository.cs`

```csharp
using MyApp.Domain.Models;

namespace MyApp.Application.Interfaces;

/// <summary>
/// Contrato de acceso a usuarios para la capa de Aplicación.
/// No se habla aquí de SQL, ni Dapper, ni EF.
/// </summary>
public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task CreateAsync(User user, CancellationToken cancellationToken);
}
```

### Implementación concreta en Infrastructure
**Archivo:** `Infrastructure/Persistence/Repositories/UserRepository.cs`

_Aquí puedes usar Dapper, EF Core, lo que quieras._

```csharp
using System.Data;
using Dapper;
using MyApp.Application.Interfaces;
using MyApp.Domain.Models;
using MyApp.Infrastructure.Persistence.Entities;
using MyApp.Infrastructure.Persistence.Mappers;

namespace MyApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación real del repositorio de usuarios.
/// Se habla SQL aquí, pero la capa superior no lo ve.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public UserRepository(IDbConnection connection, IDbTransaction? transaction = null)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var sql = "SELECT 1 FROM Users WHERE Email = @Email LIMIT 1;";
        var result = await _connection.QueryFirstOrDefaultAsync<int?>(sql, new { Email = email }, _transaction);
        return result.HasValue;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = @"SELECT Id, FullName, Email, PasswordHash, IsActive
                    FROM Users
                    WHERE Id = @Id;";

        var entity = await _connection.QueryFirstOrDefaultAsync<UserEntity>(sql, new { Id = id }, _transaction);
        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken)
    {
        var entity = UserMapper.ToEntity(user);

        var sql = @"INSERT INTO Users (Id, FullName, Email, PasswordHash, IsActive)
                    VALUES (@Id, @FullName, @Email, @PasswordHash, @IsActive);";

        await _connection.ExecuteAsync(sql, entity, _transaction);
    }
}
```

### Buenas prácticas aplicadas:
- El repositorio devuelve `Domain.Models.User`, no `UserEntity`.
- SQL está concentrado aquí, no regado por toda la app.
- Se usa `CancellationToken` en todas las funciones async.

---

## 9. Program.cs (registro de dependencias / DI)

**Archivo:** `Api/Program.cs`

```csharp
using MyApp.Application.Interfaces;
using MyApp.Application.UseCases.Users.CreateUser;
using MyApp.Infrastructure.Persistence.Repositories;
using System.Data;
using Npgsql; // Ejemplo: Postgres

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers
builder.Services.AddControllers();

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Infraestructura: conexión a BD (ejemplo Npgsql)
builder.Services.AddScoped<IDbConnection>(_ =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    var connection = new NpgsqlConnection(connString);
    connection.Open();
    return connection;
});

// 4. Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 5. Servicios de aplicación (casos de uso, hashing, jwt, etc.)
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 6. Auth / JWT / middlewares personalizados (no se muestra detalle aquí)
builder.Services.AddAuthentication(/*...*/);
builder.Services.AddAuthorization();

var app = builder.Build();

// Dev-only swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Orden recomendado de middlewares
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### Buenas prácticas aplicadas:
- Todo se registra ANTES de `app.Run();`
- Nada de `new UserRepository()` manual en los controladores.
- Conexión a base de datos como `Scoped` (por request).

---

## 10. Reglas de oro (checklist rápido)

1. **El controlador NO contiene reglas de negocio.**
2. **Cada carpeta tiene un propósito único.**
3. **Los nombres son explícitos, no abreviados.**
4. **Siempre async/await en I/O (DB, HTTP, etc.).**
5. **Usa DTOs para exponer datos, no expongas las entidades de dominio ni las entities de BD.**
6. **Mapeo dominio ↔ persistencia en un Mapper dedicado.**
7. **Evita lógica en setters públicos.** Haz métodos (`Activate()`, `Deactivate()`).
8. **Agrega XML summary `///` en clases y métodos públicos.** Eso alimenta Swagger y ayuda al mantenimiento.
9. **Reglas de negocio en Application / Domain, nunca en Infrastructure.**
10. **Todo servicio externo (correo, WhatsApp, storage, colas, etc.) debe tener interfaz (`IWhatsAppService`) y vivir detrás de esa interfaz.**

---

## 11. Anti-patrones que debes evitar SIEMPRE

❌ Controlador que hace consultas SQL directo.  
❌ Repositorio que retorna `DataTable`, `dynamic`, `any`, etc.  
❌ Usar `public set;` en todo "porque sí". Eso rompe invariantes.  
❌ Nombres como `DoStuff()`, `ProcessData()`, `HandleAsync()` sin contexto.  
❌ Poner lógica de dominio en `Program.cs`.  
❌ Poner try/catch gigantes en todos lados: centraliza manejo de errores en un middleware global.  
❌ Devolver entidades de base de datos directamente al frontend.  
❌ Usar `Guid?`/`int?` para cosas que deben existir sí o sí solo para “callar” warnings.

---

## 12. Mini resumen final

- **Api** = entrada/salida HTTP.
- **Application** = casos de uso.
- **Domain** = verdad absoluta del negocio.
- **Infrastructure** = cómo persistes y conectas con el mundo real.

Si respetas eso:
- Escalas features sin romper todo.
- Puedes testear Application y Domain SIN base de datos.
- Cambiar de Dapper a EF Core no rompe tu controlador.
- Tu código se lee como un documento, no como una pelea.

## 13. No utilizar try ni catch
-No utilizar try ni catch manejar todo con excepciones etc
