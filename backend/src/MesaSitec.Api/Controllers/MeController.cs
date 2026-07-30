using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Aplicacion.Autenticacion.Contratos;
using MesaSitec.Aplicacion.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class MeController(IAuthService authService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioResponse>> Get(
        CancellationToken cancellationToken)
    {
        var usuarioId = ObtenerGuidClaim(JwtRegisteredClaimNames.Sub);
        var tenantId = ObtenerGuidClaim("tenantId");
        var response = await authService.ObtenerUsuarioActualAsync(
            usuarioId,
            tenantId,
            cancellationToken);

        return Ok(response);
    }

    private Guid ObtenerGuidClaim(string claim)
    {
        var valor = User.FindFirstValue(claim);
        if (!Guid.TryParse(valor, out var id))
        {
            throw new NoAutenticadoException("El token no contiene los claims requeridos.");
        }

        return id;
    }
}
