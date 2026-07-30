using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Persistencia.Semillas;

public sealed class SembradorDatos(
    MesaSitecDbContext dbContext,
    IPasswordHasher<Usuario> passwordHasher)
{
    public const string PasswordSemilla = "Sitec.2026";

    private static readonly Guid TenantNorteId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static readonly Guid TenantSurId =
        Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static readonly Guid AdminNorteId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid AgenteNorte1Id =
        Guid.Parse("10000000-0000-0000-0000-000000000002");

    private static readonly Guid AgenteNorte2Id =
        Guid.Parse("10000000-0000-0000-0000-000000000003");

    private static readonly Guid UsuarioNorte1Id =
        Guid.Parse("10000000-0000-0000-0000-000000000004");

    private static readonly Guid UsuarioNorte2Id =
        Guid.Parse("10000000-0000-0000-0000-000000000005");

    private static readonly Guid AdminSurId =
        Guid.Parse("10000000-0000-0000-0000-000000000006");

    private static readonly Guid UsuarioSur1Id =
        Guid.Parse("10000000-0000-0000-0000-000000000007");

    public async Task SembrarAsync(
        DateTime fechaBase,
        CancellationToken cancellationToken = default)
    {
        if (fechaBase.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "La fecha base de la semilla debe estar expresada en UTC.",
                nameof(fechaBase));
        }

        if (await dbContext.Tenants.AnyAsync(cancellationToken))
        {
            return;
        }

        var norte = new Tenant(TenantNorteId, "Cooperativa Norte");
        var sur = new Tenant(TenantSurId, "Bufete Sur");
        var usuarios = CrearUsuarios();
        var categoriasNorte = CrearCategorias(
            TenantNorteId,
            "20000000");
        var categoriasSur = CrearCategorias(
            TenantSurId,
            "30000000");

        dbContext.Tenants.AddRange(norte, sur);
        dbContext.Usuarios.AddRange(usuarios);
        dbContext.Categorias.AddRange(categoriasNorte);
        dbContext.Categorias.AddRange(categoriasSur);

        CrearSolicitudesNorte(fechaBase, categoriasNorte);
        CrearSolicitudesSur(fechaBase, categoriasSur);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private List<Usuario> CrearUsuarios()
    {
        var usuarios = new List<Usuario>
        {
            CrearUsuario(
                AdminNorteId,
                TenantNorteId,
                "admin@norte.test",
                "Administradora Norte",
                RolUsuario.Admin),
            CrearUsuario(
                AgenteNorte1Id,
                TenantNorteId,
                "agente1@norte.test",
                "Agente Norte Uno",
                RolUsuario.Agente),
            CrearUsuario(
                AgenteNorte2Id,
                TenantNorteId,
                "agente2@norte.test",
                "Agente Norte Dos",
                RolUsuario.Agente),
            CrearUsuario(
                UsuarioNorte1Id,
                TenantNorteId,
                "user1@norte.test",
                "Solicitante Norte Uno",
                RolUsuario.Solicitante),
            CrearUsuario(
                UsuarioNorte2Id,
                TenantNorteId,
                "user2@norte.test",
                "Solicitante Norte Dos",
                RolUsuario.Solicitante),
            CrearUsuario(
                AdminSurId,
                TenantSurId,
                "admin@sur.test",
                "Administrador Sur",
                RolUsuario.Admin),
            CrearUsuario(
                UsuarioSur1Id,
                TenantSurId,
                "user1@sur.test",
                "Solicitante Sur Uno",
                RolUsuario.Solicitante)
        };

        foreach (var usuario in usuarios)
        {
            var hash = passwordHasher.HashPassword(usuario, PasswordSemilla);
            usuario.ActualizarPasswordHash(hash);
        }

        return usuarios;
    }

    private static List<Categoria> CrearCategorias(
        Guid tenantId,
        string prefijoId)
    {
        return
        [
            new(
                CrearGuid(prefijoId, 1),
                tenantId,
                "Incidente",
                8),
            new(
                CrearGuid(prefijoId, 2),
                tenantId,
                "Requerimiento",
                40),
            new(
                CrearGuid(prefijoId, 3),
                tenantId,
                "Consulta",
                24),
            new(
                CrearGuid(prefijoId, 4),
                tenantId,
                "Falla crítica",
                4)
        ];
    }

    private void CrearSolicitudesNorte(
        DateTime fechaBase,
        IReadOnlyList<Categoria> categorias)
    {
        var estados = new[]
        {
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cancelada,
            EstadoSolicitud.Cancelada,
            EstadoSolicitud.Cancelada,
            EstadoSolicitud.Cancelada
        };

        for (var indice = 0; indice < estados.Length; indice++)
        {
            var solicitanteId = indice % 2 == 0
                ? UsuarioNorte1Id
                : UsuarioNorte2Id;
            var agenteId = indice % 2 == 0
                ? AgenteNorte1Id
                : AgenteNorte2Id;

            CrearSolicitud(
                "40000000",
                indice + 1,
                TenantNorteId,
                fechaBase,
                categorias,
                solicitanteId,
                agenteId,
                estados[indice],
                "Norte");
        }
    }

    private void CrearSolicitudesSur(
        DateTime fechaBase,
        IReadOnlyList<Categoria> categorias)
    {
        var estados = new[]
        {
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cancelada,
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Asignada
        };

        for (var indice = 0; indice < estados.Length; indice++)
        {
            CrearSolicitud(
                "50000000",
                indice + 1,
                TenantSurId,
                fechaBase,
                categorias,
                UsuarioSur1Id,
                AdminSurId,
                estados[indice],
                "Sur");
        }
    }

    private void CrearSolicitud(
        string prefijoId,
        int numero,
        Guid tenantId,
        DateTime fechaBase,
        IReadOnlyList<Categoria> categorias,
        Guid solicitanteId,
        Guid agenteId,
        EstadoSolicitud estado,
        string organizacion)
    {
        var categoria = categorias[(numero - 1) % categorias.Count];
        var prioridades = Enum.GetValues<PrioridadSolicitud>();
        var prioridad = prioridades[(numero - 1) % prioridades.Length];
        var fechaCreacion = fechaBase.AddHours(numero * -12);
        var solicitud = new Solicitud(
            CrearGuid(prefijoId, numero),
            tenantId,
            $"SOL-{fechaBase.Year}-{numero:00000}",
            $"Solicitud semilla {organizacion} {numero:00}",
            $"Descripción determinista de la solicitud {numero:00} para {organizacion}.",
            categoria.Id,
            prioridad,
            solicitanteId,
            fechaCreacion,
            categoria.SlaHoras);

        dbContext.Solicitudes.Add(solicitud);
        AplicarEstadoSemilla(
            solicitud,
            estado,
            agenteId,
            fechaCreacion.AddHours(2));
    }

    private void AplicarEstadoSemilla(
        Solicitud solicitud,
        EstadoSolicitud estado,
        Guid agenteId,
        DateTime fechaResolucion)
    {
        var entry = dbContext.Entry(solicitud);
        entry.Property(item => item.Estado).CurrentValue = estado;

        if (estado is EstadoSolicitud.Asignada
            or EstadoSolicitud.EnProceso
            or EstadoSolicitud.Resuelta
            or EstadoSolicitud.Cerrada)
        {
            entry.Property(item => item.AgenteId).CurrentValue = agenteId;
        }

        if (estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
        {
            entry.Property(item => item.FechaResolucion).CurrentValue = fechaResolucion;
            entry.Property(item => item.MotivoResolucion).CurrentValue =
                "Solicitud atendida y validada correctamente.";
        }

        if (estado == EstadoSolicitud.Cancelada)
        {
            entry.Property(item => item.MotivoCancelacion).CurrentValue =
                "Solicitud duplicada en los datos semilla.";
        }
    }

    private static Usuario CrearUsuario(
        Guid id,
        Guid tenantId,
        string email,
        string nombre,
        RolUsuario rol)
    {
        return new Usuario(
            id,
            tenantId,
            email,
            "hash-pendiente",
            nombre,
            rol);
    }

    private static Guid CrearGuid(string prefijo, int numero)
    {
        return Guid.Parse($"{prefijo}-0000-0000-0000-{numero:000000000000}");
    }
}
