using MesaSitec.Aplicacion.Categorias;
using MesaSitec.Aplicacion.Categorias.Contratos;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Categorias;

public sealed class CategoriaConsultaService(
    MesaSitecDbContext dbContext) : ICategoriaConsultaService
{
    public async Task<IReadOnlyList<CategoriaResponse>> ListarActivasAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Categorias
            .AsNoTracking()
            .Where(categoria =>
                categoria.TenantId == tenantId
                && categoria.Activo)
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaResponse(
                categoria.Id,
                categoria.Nombre,
                categoria.SlaHoras))
            .ToListAsync(cancellationToken);
    }
}
