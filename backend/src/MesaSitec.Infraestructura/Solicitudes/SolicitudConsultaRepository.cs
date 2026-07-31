using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Aplicacion.Solicitudes.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class SolicitudConsultaRepository(
    MesaSitecDbContext dbContext) : ISolicitudConsultaRepository
{
    public async Task<SolicitudDetalleResponse?> ObtenerDetalleAsync(
        Guid id,
        Guid tenantId,
        DateTime ahoraUtc,
        CancellationToken cancellationToken = default)
    {
        var detalle = await (
            from solicitud in dbContext.Solicitudes
                .AsNoTracking()
                .Where(item => item.Id == id && item.TenantId == tenantId)
            join categoria in dbContext.Categorias.AsNoTracking()
                on new { solicitud.TenantId, Id = solicitud.CategoriaId }
                equals new { categoria.TenantId, categoria.Id }
            join solicitante in dbContext.Usuarios.AsNoTracking()
                on new { solicitud.TenantId, Id = solicitud.SolicitanteId }
                equals new { solicitante.TenantId, solicitante.Id }
            join agente in dbContext.Usuarios.AsNoTracking()
                on new
                {
                    solicitud.TenantId,
                    Id = solicitud.AgenteId
                }
                equals new
                {
                    agente.TenantId,
                    Id = (Guid?)agente.Id
                }
                into agentes
            from agente in agentes.DefaultIfEmpty()
            select new SolicitudDetalleResponse(
                solicitud.Id,
                solicitud.Codigo,
                solicitud.Titulo,
                solicitud.Descripcion,
                solicitud.Estado,
                solicitud.Prioridad,
                new CategoriaResumenResponse(categoria.Id, categoria.Nombre),
                new UsuarioResumenResponse(
                    solicitante.Id,
                    solicitante.Nombre),
                agente == null
                    ? null
                    : new UsuarioResumenResponse(agente.Id, agente.Nombre),
                solicitud.FechaCreacion,
                solicitud.FechaLimiteSla,
                solicitud.FechaResolucion,
                solicitud.MotivoResolucion,
                solicitud.MotivoCancelacion,
                solicitud.FechaLimiteSla < ahoraUtc
                    && solicitud.Estado != EstadoSolicitud.Resuelta
                    && solicitud.Estado != EstadoSolicitud.Cerrada
                    && solicitud.Estado != EstadoSolicitud.Cancelada))
            .SingleOrDefaultAsync(cancellationToken);

        return detalle is null
            ? null
            : detalle with
            {
                FechaCreacion = ComoUtc(detalle.FechaCreacion),
                FechaLimiteSla = ComoUtc(detalle.FechaLimiteSla),
                FechaResolucion = detalle.FechaResolucion.HasValue
                    ? ComoUtc(detalle.FechaResolucion.Value)
                    : null
            };
    }

    public async Task<PaginaResponse<SolicitudListadoResponse>> ListarAsync(
        FiltroSolicitudes filtro,
        CancellationToken cancellationToken = default)
    {
        var consulta = AplicarFiltros(
            dbContext.Solicitudes
                .AsNoTracking()
                .Where(solicitud => solicitud.TenantId == filtro.TenantId),
            filtro);
        var total = await consulta.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(total / (double)filtro.PageSize);
        if (filtro.Page > totalPaginas)
        {
            return new PaginaResponse<SolicitudListadoResponse>(
                [],
                filtro.Page,
                filtro.PageSize,
                total,
                totalPaginas);
        }

        var solicitudesPagina = Ordenar(consulta, filtro.Orden)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize);

        var itemsBase = await (
            from solicitud in solicitudesPagina
            join categoria in dbContext.Categorias.AsNoTracking()
                on new { solicitud.TenantId, Id = solicitud.CategoriaId }
                equals new { categoria.TenantId, categoria.Id }
            join agente in dbContext.Usuarios.AsNoTracking()
                on new
                {
                    solicitud.TenantId,
                    Id = solicitud.AgenteId
                }
                equals new
                {
                    agente.TenantId,
                    Id = (Guid?)agente.Id
                }
                into agentes
            from agente in agentes.DefaultIfEmpty()
            select new SolicitudListadoResponse(
                solicitud.Id,
                solicitud.Codigo,
                solicitud.Titulo,
                solicitud.Estado,
                solicitud.Prioridad,
                new CategoriaResumenResponse(categoria.Id, categoria.Nombre),
                agente == null
                    ? null
                    : new UsuarioResumenResponse(agente.Id, agente.Nombre),
                solicitud.FechaCreacion,
                solicitud.FechaLimiteSla,
                solicitud.FechaLimiteSla < filtro.AhoraUtc
                    && solicitud.Estado != EstadoSolicitud.Resuelta
                    && solicitud.Estado != EstadoSolicitud.Cerrada
                    && solicitud.Estado != EstadoSolicitud.Cancelada))
            .ToListAsync(cancellationToken);
        var items = itemsBase
            .Select(item => item with
            {
                FechaCreacion = ComoUtc(item.FechaCreacion),
                FechaLimiteSla = ComoUtc(item.FechaLimiteSla)
            })
            .ToList();
        return new PaginaResponse<SolicitudListadoResponse>(
            items,
            filtro.Page,
            filtro.PageSize,
            total,
            totalPaginas);
    }

    private static IQueryable<Solicitud> AplicarFiltros(
        IQueryable<Solicitud> consulta,
        FiltroSolicitudes filtro)
    {
        if (filtro.SolicitanteId.HasValue)
        {
            consulta = consulta.Where(
                solicitud =>
                    solicitud.SolicitanteId == filtro.SolicitanteId.Value);
        }

        if (filtro.Estado.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.Estado == filtro.Estado.Value);
        }

        if (filtro.Prioridad.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.Prioridad == filtro.Prioridad.Value);
        }

        if (filtro.CategoriaId.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.CategoriaId == filtro.CategoriaId.Value);
        }

        if (filtro.AgenteId.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.AgenteId == filtro.AgenteId.Value);
        }

        if (filtro.Busqueda is not null)
        {
            var patron = CrearPatronBusqueda(filtro.Busqueda);
            consulta = consulta.Where(
                solicitud =>
                    EF.Functions.Like(solicitud.Titulo, patron, @"\")
                    || EF.Functions.Like(solicitud.Descripcion, patron, @"\")
                    || EF.Functions.Like(solicitud.Codigo, patron, @"\"));
        }

        if (filtro.Vencidas.HasValue)
        {
            consulta = filtro.Vencidas.Value
                ? consulta.Where(
                    solicitud =>
                        solicitud.FechaLimiteSla < filtro.AhoraUtc
                        && solicitud.Estado != EstadoSolicitud.Resuelta
                        && solicitud.Estado != EstadoSolicitud.Cerrada
                        && solicitud.Estado != EstadoSolicitud.Cancelada)
                : consulta.Where(
                    solicitud =>
                        solicitud.FechaLimiteSla >= filtro.AhoraUtc
                        || solicitud.Estado == EstadoSolicitud.Resuelta
                        || solicitud.Estado == EstadoSolicitud.Cerrada
                        || solicitud.Estado == EstadoSolicitud.Cancelada);
        }

        return consulta;
    }

    private static IOrderedQueryable<Solicitud> Ordenar(
        IQueryable<Solicitud> consulta,
        string orden)
    {
        return orden switch
        {
            "fechaCreacion" => consulta
                .OrderBy(solicitud => solicitud.FechaCreacion)
                .ThenBy(solicitud => solicitud.Codigo),
            "-fechaCreacion" => consulta
                .OrderByDescending(solicitud => solicitud.FechaCreacion)
                .ThenBy(solicitud => solicitud.Codigo),
            "prioridad" => consulta
                .OrderBy(solicitud =>
                    solicitud.Prioridad == PrioridadSolicitud.Baja ? 1
                    : solicitud.Prioridad == PrioridadSolicitud.Media ? 2
                    : solicitud.Prioridad == PrioridadSolicitud.Alta ? 3
                    : 4)
                .ThenBy(solicitud => solicitud.Codigo),
            "-prioridad" => consulta
                .OrderByDescending(solicitud =>
                    solicitud.Prioridad == PrioridadSolicitud.Baja ? 1
                    : solicitud.Prioridad == PrioridadSolicitud.Media ? 2
                    : solicitud.Prioridad == PrioridadSolicitud.Alta ? 3
                    : 4)
                .ThenBy(solicitud => solicitud.Codigo),
            "codigo" => consulta.OrderBy(solicitud => solicitud.Codigo),
            _ => throw new InvalidOperationException(
                $"El orden '{orden}' no fue validado.")
        };
    }

    private static string CrearPatronBusqueda(string busqueda)
    {
        var valorEscapado = busqueda
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);

        return $"%{valorEscapado}%";
    }

    private static DateTime ComoUtc(DateTime fecha)
    {
        return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
    }
}
