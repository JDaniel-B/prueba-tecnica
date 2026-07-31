using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Excepciones;

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

    [Fact]
    public void Solicitud_ActualizarRecalculaSlaDesdeLaFechaOriginal()
    {
        var solicitud = CrearSolicitud();
        var fechaCreacionOriginal = solicitud.FechaCreacion;
        var nuevaCategoriaId = Guid.NewGuid();

        solicitud.Actualizar(
            "  Acceso al portal actualizado  ",
            "  La descripción actualizada conserva información suficiente.  ",
            nuevaCategoriaId,
            PrioridadSolicitud.Critica,
            categoriaSlaHoras: 8);

        Assert.Equal("Acceso al portal actualizado", solicitud.Titulo);
        Assert.Equal(
            "La descripción actualizada conserva información suficiente.",
            solicitud.Descripcion);
        Assert.Equal(nuevaCategoriaId, solicitud.CategoriaId);
        Assert.Equal(PrioridadSolicitud.Critica, solicitud.Prioridad);
        Assert.Equal(fechaCreacionOriginal, solicitud.FechaCreacion);
        Assert.Equal(fechaCreacionOriginal.AddHours(4), solicitud.FechaLimiteSla);
    }

    [Fact]
    public void Solicitud_ActualizarEstadoTerminalNoRecalculaSla()
    {
        var solicitud = CrearSolicitud();
        var fechaLimiteOriginal = solicitud.FechaLimiteSla;
        EstablecerEstado(solicitud, EstadoSolicitud.Resuelta);

        solicitud.Actualizar(
            "Acceso corregido después de resolver",
            "La descripción puede corregirse sin alterar el SLA histórico.",
            Guid.NewGuid(),
            PrioridadSolicitud.Critica,
            categoriaSlaHoras: 4);

        Assert.Equal(fechaLimiteOriginal, solicitud.FechaLimiteSla);
    }

    [Fact]
    public void Solicitud_RecorreFlujoFelizHastaCerrar()
    {
        var solicitud = CrearSolicitud();
        var agenteId = Guid.NewGuid();
        var fechaResolucion = FechaBase.AddHours(3);

        solicitud.Asignar(agenteId);
        Assert.Equal(EstadoSolicitud.Asignada, solicitud.Estado);
        Assert.Equal(agenteId, solicitud.AgenteId);

        solicitud.Iniciar();
        Assert.Equal(EstadoSolicitud.EnProceso, solicitud.Estado);

        solicitud.Resolver(
            "La incidencia se corrigió y fue validada por el usuario.",
            fechaResolucion);
        Assert.Equal(EstadoSolicitud.Resuelta, solicitud.Estado);
        Assert.Equal(fechaResolucion, solicitud.FechaResolucion);

        solicitud.Cerrar();
        Assert.Equal(EstadoSolicitud.Cerrada, solicitud.Estado);
    }

    [Fact]
    public void Solicitud_ReabrirLimpiaDatosDeResolucion()
    {
        var solicitud = CrearSolicitud();
        solicitud.Asignar(Guid.NewGuid());
        solicitud.Iniciar();
        solicitud.Resolver(
            "La incidencia se corrigió y fue validada por el usuario.",
            FechaBase.AddHours(3));

        solicitud.Reabrir();

        Assert.Equal(EstadoSolicitud.EnProceso, solicitud.Estado);
        Assert.Null(solicitud.FechaResolucion);
        Assert.Null(solicitud.MotivoResolucion);
    }

    [Fact]
    public void Solicitud_RechazaTransicionFueraDeLaMaquinaDeEstados()
    {
        var solicitud = CrearSolicitud();

        var accion = solicitud.Iniciar;

        Assert.Throws<TransicionSolicitudInvalidaException>(accion);
        Assert.Equal(EstadoSolicitud.Nueva, solicitud.Estado);
    }

    [Fact]
    public void Solicitud_CancelarGuardaMotivoNormalizado()
    {
        var solicitud = CrearSolicitud();

        solicitud.Cancelar("  Solicitud creada por duplicado.  ");

        Assert.Equal(EstadoSolicitud.Cancelada, solicitud.Estado);
        Assert.Equal("Solicitud creada por duplicado.", solicitud.MotivoCancelacion);
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

    private static void EstablecerEstado(
        Solicitud solicitud,
        EstadoSolicitud estado)
    {
        typeof(Solicitud)
            .GetProperty(nameof(Solicitud.Estado))!
            .SetValue(solicitud, estado);
    }
}
