namespace MesaSitec.Dominio.Entidades;

public sealed class Tenant
{
    private Tenant()
    {
    }

    public Tenant(Guid id, string nombre, bool activo = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador del tenant es obligatorio.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        Id = id;
        Nombre = nombre.Trim();
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public bool Activo { get; private set; }
}
