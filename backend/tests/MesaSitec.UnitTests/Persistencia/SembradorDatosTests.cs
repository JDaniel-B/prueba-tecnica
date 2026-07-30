using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.UnitTests.Persistencia;

public sealed class SembradorDatosTests
{
    private static readonly DateTime FechaBase =
        new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Sembrar_CreaLosDatosRequeridosYEsIdempotente()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MesaSitecDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new MesaSitecDbContext(options);
        await context.Database.MigrateAsync();
        var passwordHasher = new PasswordHasher<Usuario>();
        var sembrador = new SembradorDatos(context, passwordHasher);

        await sembrador.SembrarAsync(FechaBase);
        await sembrador.SembrarAsync(FechaBase);

        Assert.Equal(2, await context.Tenants.CountAsync());
        Assert.Equal(7, await context.Usuarios.CountAsync());
        Assert.Equal(8, await context.Categorias.CountAsync());
        Assert.Equal(33, await context.Solicitudes.CountAsync());

        var norteId = await context.Tenants
            .Where(tenant => tenant.Nombre == "Cooperativa Norte")
            .Select(tenant => tenant.Id)
            .SingleAsync();
        var surId = await context.Tenants
            .Where(tenant => tenant.Nombre == "Bufete Sur")
            .Select(tenant => tenant.Id)
            .SingleAsync();

        Assert.Equal(
            25,
            await context.Solicitudes.CountAsync(item => item.TenantId == norteId));
        Assert.Equal(
            8,
            await context.Solicitudes.CountAsync(item => item.TenantId == surId));

        var estadosNorte = await context.Solicitudes
            .Where(item => item.TenantId == norteId)
            .Select(item => item.Estado)
            .Distinct()
            .ToListAsync();
        var prioridadesNorte = await context.Solicitudes
            .Where(item => item.TenantId == norteId)
            .Select(item => item.Prioridad)
            .Distinct()
            .ToListAsync();

        Assert.All(
            Enum.GetValues<EstadoSolicitud>(),
            estado => Assert.Contains(estado, estadosNorte));
        Assert.All(
            Enum.GetValues<PrioridadSolicitud>(),
            prioridad => Assert.Contains(prioridad, prioridadesNorte));

        Assert.True(
            await context.Solicitudes.CountAsync(
                item => item.TenantId == norteId
                    && item.Estado == EstadoSolicitud.Resuelta) >= 3);

        var estadosFinales = new[]
        {
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cancelada
        };
        Assert.True(
            await context.Solicitudes.CountAsync(
                item => item.TenantId == norteId
                    && item.FechaLimiteSla < FechaBase
                    && !estadosFinales.Contains(item.Estado)) >= 5);

        var primeraSolicitudNorte = await context.Solicitudes
            .SingleAsync(item =>
                item.TenantId == norteId
                && item.Codigo == "SOL-2026-00001");
        Assert.Equal(FechaBase.AddHours(-12), primeraSolicitudNorte.FechaCreacion);

        var usuarios = await context.Usuarios.ToListAsync();
        Assert.All(
            usuarios,
            usuario =>
            {
                var resultado = passwordHasher.VerifyHashedPassword(
                    usuario,
                    usuario.PasswordHash,
                    SembradorDatos.PasswordSemilla);
                Assert.NotEqual(PasswordVerificationResult.Failed, resultado);
            });
    }
}
