using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Aplicacion.Autenticacion.Contratos;
using MesaSitec.Aplicacion.Autenticacion.Modelos;
using MesaSitec.Aplicacion.Excepciones;

namespace MesaSitec.Aplicacion.Autenticacion;

public sealed class AuthService(
    IUsuarioAutenticacionRepository usuarios,
    IPasswordVerifier passwordVerifier,
    IGeneradorTokenAcceso generadorToken) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var usuario = await usuarios.BuscarPorEmailAsync(email, cancellationToken);

        if (usuario is null
            || !usuario.Usuario.Activo
            || !usuario.TenantActivo
            || !passwordVerifier.Verificar(usuario.Usuario, request.Password ?? string.Empty))
        {
            throw new NoAutenticadoException("Credenciales incorrectas.");
        }

        var token = generadorToken.Generar(usuario);
        return new LoginResponse(
            token.Valor,
            token.ExpiraEn,
            CrearUsuarioResponse(usuario));
    }

    public async Task<UsuarioResponse> ObtenerUsuarioActualAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await usuarios.BuscarPorIdAsync(
            usuarioId,
            tenantId,
            cancellationToken);

        if (usuario is null || !usuario.Usuario.Activo || !usuario.TenantActivo)
        {
            throw new RecursoNoEncontradoException("El usuario no existe.");
        }

        return CrearUsuarioResponse(usuario);
    }

    private static UsuarioResponse CrearUsuarioResponse(
        UsuarioAutenticacion usuario)
    {
        return new UsuarioResponse(
            usuario.Usuario.Id,
            usuario.Usuario.Nombre,
            usuario.Usuario.Email,
            usuario.Usuario.Rol,
            usuario.Usuario.TenantId,
            usuario.TenantNombre);
    }
}
