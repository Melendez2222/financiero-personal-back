using FinancieroPersonal.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinancieroPersonal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<CategoriaService>();
        services.AddScoped<PeriodoService>();
        services.AddScoped<MovimientoService>();
        services.AddScoped<MetaService>();
        services.AddScoped<ConfiguracionService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<ProyeccionService>();
        services.AddScoped<DeudaService>();
        services.AddScoped<UsuarioService>();
        return services;
    }
}
