using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;
using AMR.Financeiro.Infrastructure.Repositories;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Infrastructure.Messaging;

namespace AMR.Financeiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection")!;
        services.AddDbContext<FinanceiroDbContext>(options =>
        {
            if (conn.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(conn);
            else
                options.UseSqlServer(conn);
        });

        services.AddScoped<IContaPagarRepository, ContaPagarRepository>();
        services.AddScoped<IContaReceberRepository, ContaReceberRepository>();
        services.AddScoped<ILancamentoFinanceiroRepository, LancamentoFinanceiroRepository>();
        services.AddScoped<IPlanoContasRepository, PlanoContasRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

        return services;
    }
}