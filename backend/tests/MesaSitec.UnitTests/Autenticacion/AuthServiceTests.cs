using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Aplicacion.Autenticacion.Contratos;
using MesaSitec.Aplicacion.Autenticacion.Modelos;
using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Autenticacion;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task Login_CredencialesValidasDevuelveTokenYUsuario()
    {
        var usuario = CrearUsuarioAutenticacion();
        var service = CrearServicio(usuario, passwordValido: true);

        var resultado = await service.LoginAsync(
            new LoginRequest(" agente1@norte.test ", "Sitec.2026"));

        Assert.Equal("token-de-prueba", resultado.AccessToken);
        Assert.Equal(28800, resultado.ExpiraEn);
        Assert.Equal(usuario.Usuario.Id, resultado.Usuario.Id);
        Assert.Equal(usuario.Usuario.TenantId, resultado.Usuario.TenantId);
        Assert.Equal(RolUsuario.Agente, resultado.Usuario.Rol);
        Assert.Equal("Cooperativa Norte", resultado.Usuario.TenantNombre);
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public async Task Login_RechazaPasswordUsuarioOTenantInvalido(
        bool passwordValido,
        bool usuarioActivo,
        bool tenantActivo)
    {
        var usuario = CrearUsuarioAutenticacion(usuarioActivo, tenantActivo);
        var service = CrearServicio(usuario, passwordValido);

        var accion = () => service.LoginAsync(
            new LoginRequest("agente1@norte.test", "incorrecta"));

        await Assert.ThrowsAsync<NoAutenticadoException>(accion);
    }

    [Fact]
    public async Task ObtenerUsuarioActual_ExigeIdYTenantCoincidentes()
    {
        var usuario = CrearUsuarioAutenticacion();
        var repository = new UsuarioRepositoryStub(usuario);
        var service = new AuthService(
            repository,
            new PasswordVerifierStub(true),
            new TokenGeneratorStub());

        var accion = () => service.ObtenerUsuarioActualAsync(
            usuario.Usuario.Id,
            Guid.NewGuid());

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(accion);
    }

    private static AuthService CrearServicio(
        UsuarioAutenticacion usuario,
        bool passwordValido)
    {
        return new AuthService(
            new UsuarioRepositoryStub(usuario),
            new PasswordVerifierStub(passwordValido),
            new TokenGeneratorStub());
    }

    private static UsuarioAutenticacion CrearUsuarioAutenticacion(
        bool activo = true,
        bool tenantActivo = true)
    {
        var usuario = new Usuario(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "agente1@norte.test",
            "hash-de-prueba",
            "Agente Norte Uno",
            RolUsuario.Agente,
            activo);

        return new UsuarioAutenticacion(
            usuario,
            "Cooperativa Norte",
            tenantActivo);
    }

    private sealed class UsuarioRepositoryStub(
        UsuarioAutenticacion usuario) : IUsuarioAutenticacionRepository
    {
        public Task<UsuarioAutenticacion?> BuscarPorEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            UsuarioAutenticacion? resultado =
                string.Equals(
                    email,
                    usuario.Usuario.Email,
                    StringComparison.OrdinalIgnoreCase)
                    ? usuario
                    : null;

            return Task.FromResult(resultado);
        }

        public Task<UsuarioAutenticacion?> BuscarPorIdAsync(
            Guid usuarioId,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            UsuarioAutenticacion? resultado =
                usuarioId == usuario.Usuario.Id
                && tenantId == usuario.Usuario.TenantId
                    ? usuario
                    : null;

            return Task.FromResult(resultado);
        }
    }

    private sealed class PasswordVerifierStub(bool esValido) : IPasswordVerifier
    {
        public bool Verificar(Usuario usuario, string password)
        {
            return esValido;
        }
    }

    private sealed class TokenGeneratorStub : IGeneradorTokenAcceso
    {
        public TokenAcceso Generar(UsuarioAutenticacion usuario)
        {
            return new TokenAcceso("token-de-prueba", 28800);
        }
    }
}
