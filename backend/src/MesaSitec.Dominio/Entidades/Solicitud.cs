using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Entidades;

public sealed class Solicitud
{
    public const int TituloLongitudMinima = 5;
    public const int TituloLongitudMaxima = 120;
    public const int DescripcionLongitudMinima = 10;
    public const int DescripcionLongitudMaxima = 4000;

    private Solicitud()
    {
    }

    public Solicitud(
        Guid id,
        Guid tenantId,
        string codigo,
        string titulo,
        string descripcion,
        Guid categoriaId,
        PrioridadSolicitud prioridad,
        Guid solicitanteId,
        DateTime fechaCreacion,
        DateTime fechaLimiteSla)
    {
        ValidarIdentificador(id, nameof(id));
        ValidarIdentificador(tenantId, nameof(tenantId));
        ValidarIdentificador(categoriaId, nameof(categoriaId));
        ValidarIdentificador(solicitanteId, nameof(solicitanteId));
        ValidarTexto(codigo, nameof(codigo), 1, int.MaxValue);
        ValidarTexto(
            titulo,
            nameof(titulo),
            TituloLongitudMinima,
            TituloLongitudMaxima);
        ValidarTexto(
            descripcion,
            nameof(descripcion),
            DescripcionLongitudMinima,
            DescripcionLongitudMaxima);
        ValidarFechaUtc(fechaCreacion, nameof(fechaCreacion));
        ValidarFechaUtc(fechaLimiteSla, nameof(fechaLimiteSla));

        if (fechaLimiteSla < fechaCreacion)
        {
            throw new ArgumentException(
                "La fecha límite del SLA no puede ser anterior a la creación.",
                nameof(fechaLimiteSla));
        }

        Id = id;
        TenantId = tenantId;
        Codigo = codigo.Trim();
        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        CategoriaId = categoriaId;
        Prioridad = prioridad;
        Estado = EstadoSolicitud.Nueva;
        SolicitanteId = solicitanteId;
        FechaCreacion = fechaCreacion;
        FechaLimiteSla = fechaLimiteSla;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Titulo { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public Guid CategoriaId { get; private set; }

    public PrioridadSolicitud Prioridad { get; private set; }

    public EstadoSolicitud Estado { get; private set; }

    public Guid SolicitanteId { get; private set; }

    public Guid? AgenteId { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public DateTime FechaLimiteSla { get; private set; }

    public DateTime? FechaResolucion { get; private set; }

    public string? MotivoResolucion { get; private set; }

    public string? MotivoCancelacion { get; private set; }

    private static void ValidarIdentificador(Guid id, string parametro)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador es obligatorio.", parametro);
        }
    }

    private static void ValidarTexto(
        string valor,
        string parametro,
        int longitudMinima,
        int longitudMaxima)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valor, parametro);

        var longitud = valor.Trim().Length;
        if (longitud < longitudMinima || longitud > longitudMaxima)
        {
            throw new ArgumentOutOfRangeException(
                parametro,
                $"La longitud debe estar entre {longitudMinima} y {longitudMaxima} caracteres.");
        }
    }

    private static void ValidarFechaUtc(DateTime fecha, string parametro)
    {
        if (fecha.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha debe estar expresada en UTC.", parametro);
        }
    }
}
