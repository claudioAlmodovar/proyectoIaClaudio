using System.Threading;
using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var connectionString = builder.Configuration.GetConnectionString("Consultorio")
    ?? throw new InvalidOperationException("Connection string 'Consultorio' not found.");

builder.Services.AddDbContext<ConsultorioDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", () =>
{
    try
    {
        return Results.Json(new
        {
            nombre = "API Consultorio",
            version = "1.0.0"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            message = "Ocurrió un error al obtener la información de la API.",
            detail = ex.Message
        }, statusCode: StatusCodes.Status500InternalServerError);
    }
})
   .WithName("GetRoot");

app.MapPost("/auth/login", async (ConsultorioDbContext db, LoginRequest request, CancellationToken cancellationToken) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return Results.BadRequest(new { message = "Usuario y contraseña son obligatorios." });
        }

        var normalizedUser = request.Usuario.Trim().ToLowerInvariant();

        var usuario = await db.FindUsuarioAsync(normalizedUser, cancellationToken);

        if (usuario is null || !string.Equals(usuario.Contrasena, request.Contrasena))
        {
            return Results.Json(new { message = "Credenciales inválidas." }, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!usuario.Activo)
        {
            return Results.Json(new { message = "El usuario se encuentra inactivo." }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var response = new LoginResponse(usuario.IdUsuarios, usuario.NombreUsuario, usuario.Nombre);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            message = "Ocurrió un error al iniciar sesión.",
            detail = ex.Message
        }, statusCode: StatusCodes.Status500InternalServerError);
    }
})
   .WithName("Login")
   .WithOpenApi();

app.Run();
