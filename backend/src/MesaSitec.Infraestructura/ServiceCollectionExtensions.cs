using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semillas;
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

        return services;
    }
}
