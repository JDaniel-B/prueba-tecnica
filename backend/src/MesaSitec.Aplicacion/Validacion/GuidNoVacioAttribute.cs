using System.ComponentModel.DataAnnotations;

namespace MesaSitec.Aplicacion.Validacion;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GuidNoVacioAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is null || value is Guid id && id != Guid.Empty;
    }
}
