using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Aplicacion.Autenticacion.Modelos;
using Microsoft.IdentityModel.Tokens;

namespace MesaSitec.Api.Seguridad;

public sealed class JwtTokenGenerator(
    string secret,
    TimeProvider timeProvider) : IGeneradorTokenAcceso
{
    public const string Issuer = "MesaSitec";
    public const string Audience = "MesaSitec";
    public const int ExpiracionSegundos = 8 * 60 * 60;

    private readonly SymmetricSecurityKey _signingKey =
        new(Encoding.UTF8.GetBytes(secret));

    public TokenAcceso Generar(UsuarioAutenticacion usuario)
    {
        var fechaEmision = timeProvider.GetUtcNow().UtcDateTime;
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Usuario.Id.ToString()),
            new Claim("tenantId", usuario.Usuario.TenantId.ToString()),
            new Claim("rol", usuario.Usuario.Rol.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Usuario.Email)
        };
        var credentials = new SigningCredentials(
            _signingKey,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            Issuer,
            Audience,
            claims,
            fechaEmision,
            fechaEmision.AddSeconds(ExpiracionSegundos),
            credentials);

        return new TokenAcceso(
            new JwtSecurityTokenHandler().WriteToken(token),
            ExpiracionSegundos);
    }
}
