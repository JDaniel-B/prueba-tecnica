using System.IdentityModel.Tokens.Jwt;
using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Aplicacion.Autenticacion.Contratos;
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
        var usuarioId = User.ObtenerGuidRequerido(JwtRegisteredClaimNames.Sub);
        var tenantId = User.ObtenerGuidRequerido("tenantId");
        var response = await authService.ObtenerUsuarioActualAsync(
            usuarioId,
            tenantId,
            cancellationToken);

        return Ok(response);
    }
}
