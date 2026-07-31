using MesaSitec.Aplicacion.Categorias.Contratos;

namespace MesaSitec.Aplicacion.Categorias;

public interface ICategoriaConsultaService
{
    Task<IReadOnlyList<CategoriaResponse>> ListarActivasAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
