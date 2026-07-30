using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Autenticacion.Contratos;

public sealed record UsuarioResponse(
    Guid Id,
    string Nombre,
    string Email,
    RolUsuario Rol,
    Guid TenantId,
    string TenantNombre);
