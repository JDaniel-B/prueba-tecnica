using MesaSitec.Infraestructura.Persistencia;
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

        return services;
    }
}
