using System.ComponentModel.DataAnnotations;

namespace MesaSitec.Aplicacion.Validacion;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EnumDefinidoAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is null
            || value is Enum enumValue
                && Enum.IsDefined(enumValue.GetType(), enumValue);
    }
}
