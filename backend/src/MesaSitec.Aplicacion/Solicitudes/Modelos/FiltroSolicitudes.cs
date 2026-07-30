using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes.Modelos;

public sealed record FiltroSolicitudes(
    Guid TenantId,
    Guid? SolicitanteId,
    EstadoSolicitud? Estado,
    PrioridadSolicitud? Prioridad,
    Guid? CategoriaId,
    Guid? AgenteId,
    string? Busqueda,
    bool? Vencidas,
    DateTime AhoraUtc,
    int Page,
    int PageSize,
    string Orden);
