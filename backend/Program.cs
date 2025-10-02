using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddDbContext<ConsultorioDbContext>(options =>
    options.UseInMemoryDatabase("ConsultorioDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", () => Results.Json(new
{
    nombre = "API Consultorio",
    version = "1.0.0"
}))
   .WithName("GetRoot");

app.MapPost("/auth/login", async (ConsultorioDbContext db, LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Contrasena))
    {
        return Results.BadRequest(new { message = "Usuario y contraseña son obligatorios." });
    }

    var normalizedUser = request.Usuario.Trim().ToLowerInvariant();

    var usuario = await db.Usuarios
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Usuario.ToLower() == normalizedUser);

    if (usuario is null || !string.Equals(usuario.Contrasena, request.Contrasena))
    {
        await Task.Delay(Random.Shared.Next(100, 300));
        return Results.Unauthorized();
    }

    if (!usuario.Activo)
    {
        return Results.Unauthorized(new { message = "El usuario se encuentra inactivo." });
    }

    var response = new LoginResponse(usuario.IdUsuarios, usuario.Usuario, usuario.Nombre);
    return Results.Ok(response);
})
   .WithName("Login")
   .WithOpenApi();

app.Run();
