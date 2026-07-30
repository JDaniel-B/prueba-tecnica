using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Servicios;

namespace MesaSitec.UnitTests.Dominio;

public sealed class CalculadoraSlaTests
{
    private static readonly DateTime FechaBase =
        new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(PrioridadSolicitud.Critica, 4)]
    [InlineData(PrioridadSolicitud.Alta, 6)]
    [InlineData(PrioridadSolicitud.Media, 8)]
    [InlineData(PrioridadSolicitud.Baja, 16)]
    public void Calcular_AplicaElFactorDeLaPrioridad(
        PrioridadSolicitud prioridad,
        double horasEsperadas)
    {
        var resultado = CalculadoraSla.Calcular(FechaBase, 8, prioridad);

        Assert.Equal(FechaBase.AddHours(horasEsperadas), resultado);
    }

    [Fact]
    public void Calcular_ConsultaBajaDe24HorasVenceEn48Horas()
    {
        var resultado = CalculadoraSla.Calcular(
            FechaBase,
            24,
            PrioridadSolicitud.Baja);

        Assert.Equal(FechaBase.AddHours(48), resultado);
    }

    [Fact]
    public void Calcular_RechazaFechaQueNoSeaUtc()
    {
        var fechaLocal = DateTime.SpecifyKind(FechaBase, DateTimeKind.Local);

        void Accion()
        {
            CalculadoraSla.Calcular(
                fechaLocal,
                8,
                PrioridadSolicitud.Media);
        }

        Assert.Throws<ArgumentException>(Accion);
    }
}
