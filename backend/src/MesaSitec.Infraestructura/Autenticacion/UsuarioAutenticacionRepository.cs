using System.Linq.Expressions;
using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Aplicacion.Autenticacion.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Autenticacion;

public sealed class UsuarioAutenticacionRepository(
    MesaSitecDbContext dbContext) : IUsuarioAutenticacionRepository
{
    public Task<UsuarioAutenticacion?> BuscarPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return ConsultarUsuarios(usuario => usuario.Email == email)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<UsuarioAutenticacion?> BuscarPorIdAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return ConsultarUsuarios(
                usuario => usuario.Id == usuarioId
                    && usuario.TenantId == tenantId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<UsuarioAutenticacion> ConsultarUsuarios(
        Expression<Func<Usuario, bool>> filtro)
    {
        return
            from usuario in dbContext.Usuarios.AsNoTracking().Where(filtro)
            join tenant in dbContext.Tenants.AsNoTracking()
                on usuario.TenantId equals tenant.Id
            select new UsuarioAutenticacion(
                usuario,
                tenant.Nombre,
                tenant.Activo);
    }
}
