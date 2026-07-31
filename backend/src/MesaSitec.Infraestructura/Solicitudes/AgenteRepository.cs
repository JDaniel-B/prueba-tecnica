using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public sealed class AgenteRepository(
    MesaSitecDbContext dbContext) : IAgenteRepository
{
    public Task<bool> EsValidoAsync(
        Guid agenteId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Usuarios.AnyAsync(
            usuario => usuario.Id == agenteId
                && usuario.TenantId == tenantId
                && usuario.Activo
                && (usuario.Rol == RolUsuario.Agente
                    || usuario.Rol == RolUsuario.Admin),
            cancellationToken);
    }

    public async Task<IReadOnlyList<UsuarioResumenResponse>> ListarActivosAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Usuarios
            .AsNoTracking()
            .Where(usuario => usuario.TenantId == tenantId
                && usuario.Activo
                && (usuario.Rol == RolUsuario.Agente
                    || usuario.Rol == RolUsuario.Admin))
            .OrderBy(usuario => usuario.Nombre)
            .Select(usuario => new UsuarioResumenResponse(
                usuario.Id,
                usuario.Nombre))
            .ToListAsync(cancellationToken);
    }
}
