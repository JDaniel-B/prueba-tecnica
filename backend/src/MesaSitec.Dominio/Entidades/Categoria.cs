namespace MesaSitec.Dominio.Entidades;

public sealed class Categoria
{
    private Categoria()
    {
    }

    public Categoria(
        Guid id,
        Guid tenantId,
        string nombre,
        int slaHoras,
        bool activo = true)
    {
        ValidarIdentificador(id, nameof(id));
        ValidarIdentificador(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        if (slaHoras <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slaHoras),
                "Las horas de SLA deben ser mayores que cero.");
        }

        Id = id;
        TenantId = tenantId;
        Nombre = nombre.Trim();
        SlaHoras = slaHoras;
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public int SlaHoras { get; private set; }

    public bool Activo { get; private set; }

    private static void ValidarIdentificador(Guid id, string parametro)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador es obligatorio.", parametro);
        }
    }
}
