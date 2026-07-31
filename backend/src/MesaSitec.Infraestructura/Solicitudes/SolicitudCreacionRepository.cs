using System.Globalization;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudCreacionRepository(
    MesaSitecDbContext dbContext) : ISolicitudCreacionRepository
{
    public Task<CategoriaParaSolicitud?> BuscarCategoriaActivaAsync(
        Guid categoriaId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Categorias
            .AsNoTracking()
            .Where(categoria =>
                categoria.Id == categoriaId
                && categoria.TenantId == tenantId
                && categoria.Activo)
            .Select(categoria => new CategoriaParaSolicitud(
                categoria.Id,
                categoria.SlaHoras))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> ObtenerSiguienteCorrelativoAsync(
        Guid tenantId,
        int anio,
        CancellationToken cancellationToken = default)
    {
        var prefijo = $"SOL-{anio}-";
        var ultimoCodigo = await dbContext.Solicitudes
            .AsNoTracking()
            .Where(solicitud =>
                solicitud.TenantId == tenantId
                && solicitud.Codigo.StartsWith(prefijo))
            .OrderByDescending(solicitud => solicitud.Codigo)
            .Select(solicitud => solicitud.Codigo)
            .FirstOrDefaultAsync(cancellationToken);
        if (ultimoCodigo is null)
        {
            return 1;
        }

        if (!int.TryParse(
                ultimoCodigo.AsSpan(prefijo.Length),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var ultimoCorrelativo))
        {
            throw new InvalidOperationException(
                $"El código almacenado '{ultimoCodigo}' no tiene un correlativo válido.");
        }

        return ultimoCorrelativo + 1;
    }

    public async Task GuardarAsync(
        Solicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        dbContext.Solicitudes.Add(solicitud);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
