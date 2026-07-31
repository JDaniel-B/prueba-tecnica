using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Aplicacion.Autenticacion.Abstracciones;
using MesaSitec.Aplicacion.Categorias;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.Abstracciones;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Autenticacion;
using MesaSitec.Infraestructura.Categorias;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
using MesaSitec.Infraestructura.Solicitudes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MesaSitec.Infraestructura;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfraestructura(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<MesaSitecDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        services.AddScoped<SembradorDatos>();
        services.AddScoped<IUsuarioAutenticacionRepository, UsuarioAutenticacionRepository>();
        services.AddScoped<IPasswordVerifier, PasswordVerifier>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoriaConsultaService, CategoriaConsultaService>();
        services.AddScoped<ISolicitudConsultaRepository, SolicitudConsultaRepository>();
        services.AddScoped<ISolicitudConsultaService, SolicitudConsultaService>();
        services.AddScoped<ISolicitudEscrituraRepository, SolicitudEscrituraRepository>();
        services.AddScoped<ISolicitudCreacionService, SolicitudCreacionService>();
        services.AddScoped<ISolicitudActualizacionService, SolicitudActualizacionService>();
        services.AddScoped<ISolicitudTransicionService, SolicitudTransicionService>();
        services.AddScoped<IAgenteRepository, AgenteRepository>();
        services.AddScoped<IAgenteConsultaService, AgenteConsultaService>();

        return services;
    }
}
