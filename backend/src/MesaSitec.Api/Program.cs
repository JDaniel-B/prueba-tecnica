using System.Globalization;
using MesaSitec.Infraestructura;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddInfraestructura(connectionString);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
app.UseCors("Frontend");
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
