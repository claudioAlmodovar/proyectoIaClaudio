using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

class ConsultorioDbContext : DbContext
{
    public ConsultorioDbContext(DbContextOptions<ConsultorioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                IdUsuarios = 1,
                Usuario = "recepcion",
                Nombre = "Laura Sánchez",
                Contrasena = "recepcion123",
                Activo = true
            },
            new Usuario
            {
                IdUsuarios = 2,
                Usuario = "doctor1",
                Nombre = "Dr. Jorge Medina",
                Contrasena = "consulta2024",
                Activo = true
            },
            new Usuario
            {
                IdUsuarios = 3,
                Usuario = "admin",
                Nombre = "Administración",
                Contrasena = "admin2024",
                Activo = false
            });
    }
}

class Usuario
{
    public int IdUsuarios { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public bool Activo { get; set; }
}

record LoginRequest(string Usuario, string Contrasena);

record LoginResponse(int Id, string Usuario, string Nombre);
