using System.IdentityModel.Tokens.Jwt;
using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Autenticacion.Modelos;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using Microsoft.IdentityModel.Tokens;

namespace MesaSitec.UnitTests.Autenticacion;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void Generar_IncluyeClaimsAlgoritmoYExpiracionRequeridos()
    {
        var fechaBase = new DateTimeOffset(
            2026,
            1,
            15,
            8,
            0,
            0,
            TimeSpan.Zero);
        var usuario = new Usuario(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "agente1@norte.test",
            "hash-de-prueba",
            "Agente Norte Uno",
            RolUsuario.Agente);
        var usuarioAuth = new UsuarioAutenticacion(
            usuario,
            "Cooperativa Norte",
            true);
        var generator = new JwtTokenGenerator(
            "secreto-de-prueba-con-mas-de-32-caracteres-123456",
            new FixedTimeProvider(fechaBase));

        var resultado = generator.Generar(usuarioAuth);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(resultado.Valor);

        Assert.Equal(SecurityAlgorithms.HmacSha256, token.Header.Alg);
        Assert.Equal(usuario.Id.ToString(), token.Subject);
        Assert.Equal(
            usuario.TenantId.ToString(),
            token.Claims.Single(claim => claim.Type == "tenantId").Value);
        Assert.Equal(
            "Agente",
            token.Claims.Single(claim => claim.Type == "rol").Value);
        Assert.Equal(
            usuario.Email,
            token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(28800, resultado.ExpiraEn);
        Assert.Equal(fechaBase.UtcDateTime.AddHours(8), token.ValidTo);
    }

    private sealed class FixedTimeProvider(DateTimeOffset fecha) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return fecha;
        }
    }
}
