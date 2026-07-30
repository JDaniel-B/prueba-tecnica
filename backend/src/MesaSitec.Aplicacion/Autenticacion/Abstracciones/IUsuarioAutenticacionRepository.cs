using MesaSitec.Aplicacion.Autenticacion.Modelos;

namespace MesaSitec.Aplicacion.Autenticacion.Abstracciones;

public interface IUsuarioAutenticacionRepository
{
    Task<UsuarioAutenticacion?> BuscarPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<UsuarioAutenticacion?> BuscarPorIdAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
