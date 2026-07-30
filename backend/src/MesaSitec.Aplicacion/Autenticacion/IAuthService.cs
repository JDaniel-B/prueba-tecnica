using MesaSitec.Aplicacion.Autenticacion.Contratos;

namespace MesaSitec.Aplicacion.Autenticacion;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<UsuarioResponse> ObtenerUsuarioActualAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
