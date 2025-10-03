using System.Text;
using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Backend.Options;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<ConsultorioDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("Consultorio")));

builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();

var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                 ?? throw new InvalidOperationException("La configuración de JWT es obligatoria.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException("La clave JWT no está configurada.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Consultorio API",
        Version = "v1",
        Description = "API minimal para gestionar consultorios médicos"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT usando el esquema Bearer"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseCors("AllowAll");
app.UseAuthorization();

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
        return Results.Problem($"Ocurrió un error al obtener la información de la API. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("GetRoot");

app.MapPost("/auth/login", async (
    LoginRequest request,
    ConsultorioDbContext db,
    IPasswordService passwordService,
    IJwtTokenService tokenService) =>
{
    try
    {
        var correoNormalizado = request.Correo.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correoNormalizado);

        if (usuario is null)
        {
            return Results.Json(new { message = "Credenciales inválidas." }, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!usuario.Activo)
        {
            return Results.Json(new { message = "El usuario se encuentra inactivo." }, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!passwordService.VerifyPassword(usuario.PasswordHash, request.Password))
        {
            return Results.Json(new { message = "Credenciales inválidas." }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var token = tokenService.GenerateToken(usuario);
        var response = new LoginResponse(
            token.Token,
            token.Expiration,
            new UsuarioSummary(usuario.Id, usuario.Correo, usuario.NombreCompleto, usuario.MedicoId));

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al iniciar sesión. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("Login").AllowAnonymous();

app.MapPost("/api/usuarios", async (
    CreateUsuarioRequest request,
    ConsultorioDbContext db,
    IPasswordService passwordService) =>
{
    try
    {
        var correoNormalizado = request.Correo.Trim().ToLowerInvariant();
        var existeUsuario = await db.Usuarios.AnyAsync(u => u.Correo.ToLower() == correoNormalizado);
        if (existeUsuario)
        {
            return Results.BadRequest(new { message = "El correo electrónico ya se encuentra registrado." });
        }

        if (request.MedicoId.HasValue)
        {
            var medicoExiste = await db.Medicos.AnyAsync(m => m.Id == request.MedicoId.Value);
            if (!medicoExiste)
            {
                return Results.BadRequest(new { message = "El médico asociado no existe." });
            }
        }

        var nuevoUsuario = new Usuario
        {
            Correo = request.Correo.Trim(),
            PasswordHash = passwordService.HashPassword(request.Password),
            NombreCompleto = request.NombreCompleto.Trim(),
            MedicoId = request.MedicoId,
            Activo = request.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        db.Usuarios.Add(nuevoUsuario);
        await db.SaveChangesAsync();

        var response = new UsuarioResponse(
            nuevoUsuario.Id,
            nuevoUsuario.Correo,
            nuevoUsuario.NombreCompleto,
            nuevoUsuario.MedicoId,
            nuevoUsuario.Activo,
            nuevoUsuario.FechaCreacion);

        return Results.Created($"/api/usuarios/{nuevoUsuario.Id}", response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al crear el usuario. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("CreateUsuario").AllowAnonymous();

app.MapGet("/api/usuarios", async (ConsultorioDbContext db) =>
{
    try
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .Select(u => new UsuarioResponse(u.Id, u.Correo, u.NombreCompleto, u.MedicoId, u.Activo, u.FechaCreacion))
            .ToListAsync();

        return Results.Ok(usuarios);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al consultar los usuarios. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("GetUsuarios").RequireAuthorization();

app.MapGet("/api/usuarios/{id:int}", async (int id, ConsultorioDbContext db) =>
{
    try
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UsuarioResponse(u.Id, u.Correo, u.NombreCompleto, u.MedicoId, u.Activo, u.FechaCreacion))
            .FirstOrDefaultAsync();

        return usuario is null
            ? Results.NotFound()
            : Results.Ok(usuario);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al consultar el usuario. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("GetUsuario").RequireAuthorization();

app.MapDelete("/api/usuarios/{id:int}", async (int id, ConsultorioDbContext db) =>
{
    try
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            return Results.NotFound();
        }

        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al eliminar el usuario. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("DeleteUsuario").RequireAuthorization();

app.MapPost("/api/medicos", async (CreateMedicoRequest request, ConsultorioDbContext db) =>
{
    try
    {
        var cedulaExiste = await db.Medicos.AnyAsync(m => m.Cedula == request.Cedula);
        if (cedulaExiste)
        {
            return Results.BadRequest(new { message = "La cédula ya se encuentra registrada." });
        }

        var emailExiste = !string.IsNullOrWhiteSpace(request.Email) && await db.Medicos.AnyAsync(m => m.Email == request.Email);
        if (emailExiste)
        {
            return Results.BadRequest(new { message = "El correo electrónico del médico ya se encuentra registrado." });
        }

        var medico = new Medico
        {
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = request.SegundoNombre?.Trim(),
            ApellidoPaterno = request.ApellidoPaterno.Trim(),
            ApellidoMaterno = request.ApellidoMaterno?.Trim(),
            Cedula = request.Cedula.Trim(),
            Telefono = request.Telefono?.Trim(),
            Especialidad = request.Especialidad?.Trim(),
            Email = request.Email?.Trim(),
            Activo = request.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        db.Medicos.Add(medico);
        await db.SaveChangesAsync();

        var response = new MedicoResponse(
            medico.Id,
            medico.PrimerNombre,
            medico.SegundoNombre,
            medico.ApellidoPaterno,
            medico.ApellidoMaterno,
            medico.Cedula,
            medico.Telefono,
            medico.Especialidad,
            medico.Email,
            medico.Activo,
            medico.FechaCreacion);

        return Results.Created($"/api/medicos/{medico.Id}", response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al crear el médico. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("CreateMedico").RequireAuthorization();

app.MapGet("/api/medicos", async (ConsultorioDbContext db) =>
{
    try
    {
        var medicos = await db.Medicos
            .AsNoTracking()
            .Select(m => new MedicoResponse(
                m.Id,
                m.PrimerNombre,
                m.SegundoNombre,
                m.ApellidoPaterno,
                m.ApellidoMaterno,
                m.Cedula,
                m.Telefono,
                m.Especialidad,
                m.Email,
                m.Activo,
                m.FechaCreacion))
            .ToListAsync();

        return Results.Ok(medicos);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al consultar los médicos. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("GetMedicos").RequireAuthorization();

app.MapPut("/api/medicos/{id:int}", async (int id, UpdateMedicoRequest request, ConsultorioDbContext db) =>
{
    try
    {
        var medico = await db.Medicos.FindAsync(id);
        if (medico is null)
        {
            return Results.NotFound();
        }

        var cedulaExiste = await db.Medicos.AnyAsync(m => m.Id != id && m.Cedula == request.Cedula);
        if (cedulaExiste)
        {
            return Results.BadRequest(new { message = "La cédula ya se encuentra registrada." });
        }

        var emailTrimmed = request.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(emailTrimmed))
        {
            var emailExiste = await db.Medicos.AnyAsync(m => m.Id != id && m.Email == emailTrimmed);
            if (emailExiste)
            {
                return Results.BadRequest(new { message = "El correo electrónico del médico ya se encuentra registrado." });
            }
        }

        medico.PrimerNombre = request.PrimerNombre.Trim();
        medico.SegundoNombre = string.IsNullOrWhiteSpace(request.SegundoNombre) ? null : request.SegundoNombre.Trim();
        medico.ApellidoPaterno = request.ApellidoPaterno.Trim();
        medico.ApellidoMaterno = string.IsNullOrWhiteSpace(request.ApellidoMaterno) ? null : request.ApellidoMaterno.Trim();
        medico.Cedula = request.Cedula.Trim();
        medico.Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? null : request.Telefono.Trim();
        medico.Especialidad = string.IsNullOrWhiteSpace(request.Especialidad) ? null : request.Especialidad.Trim();
        medico.Email = string.IsNullOrWhiteSpace(emailTrimmed) ? null : emailTrimmed;
        medico.Activo = request.Activo;

        await db.SaveChangesAsync();

        var response = new MedicoResponse(
            medico.Id,
            medico.PrimerNombre,
            medico.SegundoNombre,
            medico.ApellidoPaterno,
            medico.ApellidoMaterno,
            medico.Cedula,
            medico.Telefono,
            medico.Especialidad,
            medico.Email,
            medico.Activo,
            medico.FechaCreacion);

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al actualizar el médico. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("UpdateMedico").RequireAuthorization();

app.MapDelete("/api/medicos/{id:int}", async (int id, ConsultorioDbContext db) =>
{
    try
    {
        var medico = await db.Medicos.FindAsync(id);
        if (medico is null)
        {
            return Results.NotFound();
        }

        db.Medicos.Remove(medico);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al eliminar el médico. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("DeleteMedico").RequireAuthorization();

app.MapPost("/api/pacientes", async (CreatePacienteRequest request, ConsultorioDbContext db) =>
{
    try
    {
        var paciente = new Paciente
        {
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = request.SegundoNombre?.Trim(),
            ApellidoPaterno = request.ApellidoPaterno.Trim(),
            ApellidoMaterno = request.ApellidoMaterno?.Trim(),
            Telefono = request.Telefono?.Trim(),
            Activo = request.Activo,
            FechaCreacion = DateTime.UtcNow
        };

        db.Pacientes.Add(paciente);
        await db.SaveChangesAsync();

        var response = new PacienteResponse(
            paciente.Id,
            paciente.PrimerNombre,
            paciente.SegundoNombre,
            paciente.ApellidoPaterno,
            paciente.ApellidoMaterno,
            paciente.Telefono,
            paciente.Activo,
            paciente.FechaCreacion);

        return Results.Created($"/api/pacientes/{paciente.Id}", response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al crear el paciente. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("CreatePaciente").RequireAuthorization();

app.MapGet("/api/pacientes", async (ConsultorioDbContext db) =>
{
    try
    {
        var pacientes = await db.Pacientes
            .AsNoTracking()
            .Select(p => new PacienteResponse(
                p.Id,
                p.PrimerNombre,
                p.SegundoNombre,
                p.ApellidoPaterno,
                p.ApellidoMaterno,
                p.Telefono,
                p.Activo,
                p.FechaCreacion))
            .ToListAsync();

        return Results.Ok(pacientes);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al consultar los pacientes. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("GetPacientes").RequireAuthorization();

app.MapDelete("/api/pacientes/{id:int}", async (int id, ConsultorioDbContext db) =>
{
    try
    {
        var paciente = await db.Pacientes.FindAsync(id);
        if (paciente is null)
        {
            return Results.NotFound();
        }

        db.Pacientes.Remove(paciente);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al eliminar el paciente. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("DeletePaciente").RequireAuthorization();

app.MapPost("/api/consultas", async (CreateConsultaRequest request, ConsultorioDbContext db) =>
{
    try
    {
        var medicoExiste = await db.Medicos.AnyAsync(m => m.Id == request.MedicoId);
        if (!medicoExiste)
        {
            return Results.BadRequest(new { message = "El médico especificado no existe." });
        }

        var pacienteExiste = await db.Pacientes.AnyAsync(p => p.Id == request.PacienteId);
        if (!pacienteExiste)
        {
            return Results.BadRequest(new { message = "El paciente especificado no existe." });
        }

        var consulta = new Consulta
        {
            MedicoId = request.MedicoId,
            PacienteId = request.PacienteId,
            FechaConsulta = request.FechaConsulta,
            Sintomas = request.Sintomas?.Trim(),
            Recomendaciones = request.Recomendaciones?.Trim(),
            Diagnostico = request.Diagnostico?.Trim()
        };

        db.Consultas.Add(consulta);
        await db.SaveChangesAsync();

        var response = new ConsultaResponse(
            consulta.Id,
            consulta.MedicoId,
            consulta.PacienteId,
            consulta.FechaConsulta,
            consulta.Sintomas,
            consulta.Recomendaciones,
            consulta.Diagnostico);

        return Results.Created($"/api/consultas/{consulta.Id}", response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al crear la consulta médica. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("CreateConsulta").RequireAuthorization();

app.MapGet("/api/consultas", async (ConsultorioDbContext db) =>
{
    try
    {
        var consultas = await db.Consultas
            .AsNoTracking()
            .Select(c => new ConsultaResponse(
                c.Id,
                c.MedicoId,
                c.PacienteId,
                c.FechaConsulta,
                c.Sintomas,
                c.Recomendaciones,
                c.Diagnostico))
            .ToListAsync();

        return Results.Ok(consultas);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al consultar las consultas médicas. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("GetConsultas").RequireAuthorization();

app.MapDelete("/api/consultas/{id:int}", async (int id, ConsultorioDbContext db) =>
{
    try
    {
        var consulta = await db.Consultas.FindAsync(id);
        if (consulta is null)
        {
            return Results.NotFound();
        }

        db.Consultas.Remove(consulta);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocurrió un error al eliminar la consulta médica. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("DeleteConsulta").RequireAuthorization();

app.Run();
