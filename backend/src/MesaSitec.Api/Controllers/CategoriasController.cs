using MesaSitec.Api.Seguridad;
using MesaSitec.Aplicacion.Categorias;
using MesaSitec.Aplicacion.Categorias.Contratos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categorias")]
public sealed class CategoriasController(
    ICategoriaConsultaService consultaService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoriaResponse>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CategoriaResponse>>> Get(
        CancellationToken cancellationToken)
    {
        var tenantId = User.ObtenerGuidRequerido("tenantId");
        var response = await consultaService.ListarActivasAsync(
            tenantId,
            cancellationToken);

        return Ok(response);
    }
}
