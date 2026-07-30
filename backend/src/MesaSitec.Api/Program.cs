using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using MesaSitec.Api.Errores;
using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Infraestructura;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'DefaultConnection'.");
var jwtSecret = ObtenerJwtSecret(builder.Configuration);

builder.Services.AddInfraestructura(connectionString);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGeneradorTokenAcceso>(provider =>
    new JwtTokenGenerator(
        jwtSecret,
        provider.GetRequiredService<TimeProvider>()));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errores = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(
                item => string.IsNullOrEmpty(item.Key)
                    ? "body"
                    : char.ToLowerInvariant(item.Key[0]) + item.Key[1..],
                item => item.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "El valor enviado no es válido."
                        : error.ErrorMessage)
                    .ToArray());
        var problema = new
        {
            type = "https://mesasitec.local/errores/validacion",
            title = "Error de validación",
            status = StatusCodes.Status422UnprocessableEntity,
            detail = "Uno o más campos no son válidos.",
            codigo = "VALIDACION",
            errores
        };

        return new ObjectResult(problema)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity,
            ContentTypes = { "application/problem+json" }
        };
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Escribe el token JWT sin comillas.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtTokenGenerator.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtTokenGenerator.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await ProblemaApi.EscribirAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "NO_AUTENTICADO",
                    "No autenticado",
                    "Se requiere un token válido para acceder al recurso.",
                    "no-autenticado");
            },
            OnForbidden = context => ProblemaApi.EscribirAsync(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                "OPERACION_NO_PERMITIDA",
                "Operación no permitida",
                "El usuario no tiene permiso para realizar esta operación.",
                "operacion-no-permitida")
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    await dbContext.Database.MigrateAsync();

    var sembrador = scope.ServiceProvider.GetRequiredService<SembradorDatos>();
    await sembrador.SembrarAsync(ObtenerFechaBaseSemilla(builder.Configuration));
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseExceptionHandler();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static DateTime ObtenerFechaBaseSemilla(IConfiguration configuration)
{
    const string fechaBasePredeterminada = "2026-01-15T08:00:00Z";
    var valor = configuration["SEED_FECHA_BASE"] ?? fechaBasePredeterminada;

    if (!DateTimeOffset.TryParse(
            valor,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var fechaBase))
    {
        throw new InvalidOperationException(
            "SEED_FECHA_BASE debe ser una fecha ISO-8601 válida.");
    }

    return fechaBase.UtcDateTime;
}

static string ObtenerJwtSecret(IConfiguration configuration)
{
    var secret = configuration["JWT_SECRET"];

    if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
    {
        throw new InvalidOperationException(
            "JWT_SECRET debe definirse con al menos 32 caracteres.");
    }

    return secret;
}
