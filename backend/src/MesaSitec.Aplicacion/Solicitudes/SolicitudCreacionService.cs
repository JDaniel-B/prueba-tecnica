using System.ComponentModel.DataAnnotations;
using MesaSitec.Aplicacion.Excepciones;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Aplicacion.Solicitudes.Contratos;
using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public sealed class SolicitudCreacionService(
    ISolicitudCreacionRepository creacionRepository,
    ISolicitudConsultaRepository consultaRepository,
    TimeProvider timeProvider) : ISolicitudCreacionService
{
    public async Task<SolicitudDetalleResponse> CrearAsync(
        CrearSolicitudRequest request,
        Guid tenantId,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        ValidarValores(request);

        var categoria = await creacionRepository.BuscarCategoriaActivaAsync(
            request.CategoriaId!.Value,
            tenantId,
            cancellationToken);
        if (categoria is null)
        {
            throw CrearError(
                "categoriaId",
                "La categoría no existe, está inactiva o pertenece a otra organización.");
        }

        var fechaCreacion = timeProvider.GetUtcNow().UtcDateTime;
        var correlativo = await creacionRepository.ObtenerSiguienteCorrelativoAsync(
            tenantId,
            fechaCreacion.Year,
            cancellationToken);
        if (correlativo > 99999)
        {
            throw new InvalidOperationException(
                "Se agotó el correlativo anual de solicitudes.");
        }

        var solicitud = new Solicitud(
            Guid.NewGuid(),
            tenantId,
            $"SOL-{fechaCreacion.Year}-{correlativo:00000}",
            request.Titulo!,
            request.Descripcion!,
            categoria.Id,
            request.Prioridad!.Value,
            usuarioId,
            fechaCreacion,
            categoria.SlaHoras);
        await creacionRepository.GuardarAsync(solicitud, cancellationToken);

        return await consultaRepository.ObtenerDetalleAsync(
                solicitud.Id,
                tenantId,
                fechaCreacion,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "No fue posible recuperar la solicitud creada.");
    }

    private static void ValidarValores(CrearSolicitudRequest request)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            resultados,
            validateAllProperties: true);
        var errores = resultados
            .SelectMany(resultado =>
            {
                var campos = resultado.MemberNames.Any()
                    ? resultado.MemberNames
                    : ["body"];

                return campos.Select(campo => new
                {
                    Campo = ComoCamelCase(campo),
                    Mensaje = resultado.ErrorMessage
                        ?? "El valor enviado no es válido."
                });
            })
            .GroupBy(item => item.Campo)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(item => item.Mensaje).ToArray());

        if (request.Titulo is not null
            && request.Titulo.Trim().Length
                is < Solicitud.TituloLongitudMinima
                or > Solicitud.TituloLongitudMaxima)
        {
            errores["titulo"] =
                ["El título debe tener entre 5 y 120 caracteres."];
        }

        if (request.Descripcion is not null
            && request.Descripcion.Trim().Length
                is < Solicitud.DescripcionLongitudMinima
                or > Solicitud.DescripcionLongitudMaxima)
        {
            errores["descripcion"] =
                ["La descripción debe tener entre 10 y 4000 caracteres."];
        }

        if (request.CategoriaId == Guid.Empty)
        {
            errores["categoriaId"] = ["La categoría es obligatoria."];
        }

        if (request.Prioridad.HasValue
            && !Enum.IsDefined(request.Prioridad.Value))
        {
            errores["prioridad"] =
                ["La prioridad no contiene un valor permitido."];
        }

        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }
    }

    private static ValidacionException CrearError(
        string campo,
        string mensaje)
    {
        return new ValidacionException(
            new Dictionary<string, string[]>
            {
                [campo] = [mensaje]
            });
    }

    private static string ComoCamelCase(string valor)
    {
        return string.IsNullOrEmpty(valor)
            ? "body"
            : char.ToLowerInvariant(valor[0]) + valor[1..];
    }
}
