using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Solicitudes.Contratos;

public sealed class ListarSolicitudesRequest
{
    public EstadoSolicitud? Estado { get; init; }

    public PrioridadSolicitud? Prioridad { get; init; }

    public Guid? CategoriaId { get; init; }

    public Guid? AgenteId { get; init; }

    public string? Q { get; init; }

    public bool? Vencidas { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string Sort { get; init; } = "-fechaCreacion";
}
