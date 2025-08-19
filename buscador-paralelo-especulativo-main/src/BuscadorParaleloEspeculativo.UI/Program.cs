using BuscadorParaleloEspeculativo.UI.Models;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios necesarios
builder.Services.AddRazorPages();
builder.Services.AddControllers(); 

// Configurar logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});

// Registrar dependencias como Singleton para mantener estado entre requests
builder.Services.AddSingleton<ProcesadorArchivos>();
builder.Services.AddSingleton<ModeloPrediccion>();

// Configurar JSON para evitar problemas de serialización
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Agregar servicios para manejo de archivos grandes
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
});

// Configurar CORS si es necesario
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(); // Si configuraste CORS

app.UseAuthorization();

//Mapear tanto Razor Pages como Controllers
app.MapRazorPages();
app.MapControllers();// Esto es para que funcionen los endpoints /api/

// Logging de inicio
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Sistema de Predicción de Texto Especulativo iniciado");
logger.LogInformation("Endpoints disponibles:");
logger.LogInformation("- POST /api/archivos/procesar");
logger.LogInformation("- GET  /api/archivos/estado");
logger.LogInformation("- POST /api/archivos/limpiar");
logger.LogInformation("- POST /api/prediccion/predecir");
logger.LogInformation("- GET  /api/prediccion/estadisticas");
logger.LogInformation("- POST /api/prediccion/buscar-prefijo");
logger.LogInformation("- POST /api/prediccion/validar-contexto");

app.Run();