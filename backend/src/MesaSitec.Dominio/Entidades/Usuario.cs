using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Entidades;

public sealed class Usuario
{
    private Usuario()
    {
    }

    public Usuario(
        Guid id,
        Guid tenantId,
        string email,
        string passwordHash,
        string nombre,
        RolUsuario rol,
        bool activo = true)
    {
        ValidarIdentificador(id, nameof(id));
        ValidarIdentificador(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        Id = id;
        TenantId = tenantId;
        Email = email.Trim();
        PasswordHash = passwordHash;
        Nombre = nombre.Trim();
        Rol = rol;
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public RolUsuario Rol { get; private set; }

    public bool Activo { get; private set; }

    public void ActualizarPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    private static void ValidarIdentificador(Guid id, string parametro)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador es obligatorio.", parametro);
        }
    }
}
