using MesaSitec.Aplicacion;
using MesaSitec.Dominio;

namespace MesaSitec.UnitTests;

public sealed class ArquitecturaTests
{
    [Fact]
    public void CapasBase_SonReferenciablesDesdeLasPruebas()
    {
        Assert.NotNull(typeof(IAplicacionMarker).Assembly);
        Assert.NotNull(typeof(IDominioMarker).Assembly);
    }
}
