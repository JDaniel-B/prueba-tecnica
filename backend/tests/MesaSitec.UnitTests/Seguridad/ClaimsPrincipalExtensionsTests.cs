using System.Security.Claims;
using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Seguridad;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void ClaimsValidos_DevuelvenGuidYRol()
    {
        var tenantId = Guid.NewGuid();
        var principal = CrearPrincipal(
            new Claim("tenantId", tenantId.ToString()),
            new Claim("rol", RolUsuario.Agente.ToString()));

        var tenantObtenido = principal.ObtenerGuidRequerido("tenantId");
        var rolObtenido = principal.ObtenerRolRequerido();

        Assert.Equal(tenantId, tenantObtenido);
        Assert.Equal(RolUsuario.Agente, rolObtenido);
    }

    [Theory]
    [InlineData("tenantId", "valor-invalido")]
    [InlineData("tenantId", "00000000-0000-0000-0000-000000000000")]
    [InlineData("tenantId", "")]
    public void GuidAusenteOInvalido_LanzaNoAutenticado(
        string claim,
        string valor)
    {
        var claims = string.IsNullOrEmpty(valor)
            ? Array.Empty<Claim>()
            : [new Claim(claim, valor)];
        var principal = CrearPrincipal(claims);

        Assert.Throws<NoAutenticadoException>(
            () => principal.ObtenerGuidRequerido(claim));
    }

    [Theory]
    [InlineData("rol-invalido")]
    [InlineData("99")]
    public void RolInvalido_LanzaNoAutenticado(string valor)
    {
        var principal = CrearPrincipal(new Claim("rol", valor));

        Assert.Throws<NoAutenticadoException>(
            () => principal.ObtenerRolRequerido());
    }

    private static ClaimsPrincipal CrearPrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "prueba"));
    }
}
