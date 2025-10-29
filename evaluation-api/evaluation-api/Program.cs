using Microsoft.EntityFrameworkCore;
using evaluation_api.Infrastructure.Persistence;
using evaluation_api.Infrastructure.Persistence.Repositories;
using evaluation_api.Application.Interfaces;
using evaluation_api.Application.Services;
using evaluation_api.Application.UseCases.Analysis;
using evaluation_api.Infrastructure.AI.Adapters;
using evaluation_api.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Evaluation IA API",
        Version = "v1",
        Description = "API para análisis de especificaciones de software usando IA"
    });
});

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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IProcesoRepository, ProcesoRepository>();
builder.Services.AddScoped<ISubprocesoRepository, SubprocesoRepository>();
builder.Services.AddScoped<ICasoUsoRepository, CasoUsoRepository>();

builder.Services.AddHttpClient<IAIService, OpenRouterAdapter>();

builder.Services.AddScoped<ISpecificationAnalysisService, SpecificationAnalysisService>();

builder.Services.AddScoped<AnalyzeSpecificationHandler>();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseExceptionHandling();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Evaluation IA API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthorization();

app.MapControllers();
app.Logger.LogInformation("🚀 Evaluation IA API iniciada");
app.Logger.LogInformation("📊 Swagger UI disponible en: http://localhost:5000");
app.Logger.LogInformation("🔗 API URL: http://localhost:5000/api/analysis/analyze");

app.Run();
