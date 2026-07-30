using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.UnitTests.Dominio;

public sealed class EntidadesTests
{
    private static readonly DateTime FechaBase =
        new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Solicitud_NuevaIniciaSinAgenteNiDatosDeCierre()
    {
        var solicitud = CrearSolicitud();

        Assert.Equal(EstadoSolicitud.Nueva, solicitud.Estado);
        Assert.Null(solicitud.AgenteId);
        Assert.Null(solicitud.FechaResolucion);
        Assert.Null(solicitud.MotivoResolucion);
        Assert.Null(solicitud.MotivoCancelacion);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901"
        + "234567890123456789012345678901234567890123456789012345678901")]
    public void Solicitud_RechazaTituloFueraDelRangoPermitido(string titulo)
    {
        var accion = () => CrearSolicitud(titulo);

        Assert.Throws<ArgumentOutOfRangeException>(accion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Categoria_RechazaSlaNoPositivo(int slaHoras)
    {
        var accion = () => new Categoria(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Incidente",
            slaHoras);

        Assert.Throws<ArgumentOutOfRangeException>(accion);
    }

    [Fact]
    public void Solicitud_RechazaFechasQueNoSeanUtc()
    {
        var accion = () => new Solicitud(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SOL-2026-00001",
            "No puedo acceder al portal",
            "El portal rechaza mis credenciales de acceso.",
            Guid.NewGuid(),
            PrioridadSolicitud.Alta,
            Guid.NewGuid(),
            DateTime.SpecifyKind(FechaBase, DateTimeKind.Local),
            8);

        Assert.Throws<ArgumentException>(accion);
    }

    private static Solicitud CrearSolicitud(string titulo = "No puedo acceder al portal")
    {
        return new Solicitud(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SOL-2026-00001",
            titulo,
            "El portal rechaza mis credenciales de acceso.",
            Guid.NewGuid(),
            PrioridadSolicitud.Alta,
            Guid.NewGuid(),
            FechaBase,
            8);
    }
}
