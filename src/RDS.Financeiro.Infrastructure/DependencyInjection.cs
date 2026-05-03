using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RDS.Financeiro.Domain.Interfaces;
using RDS.Financeiro.Infrastructure.Data;
using RDS.Financeiro.Infrastructure.Repositories;

namespace RDS.Financeiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FinanceiroDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IContaPagarRepository, ContaPagarRepository>();
        services.AddScoped<IContaReceberRepository, ContaReceberRepository>();
        services.AddScoped<IPlanoContasRepository, PlanoContasRepository>();
        services.AddScoped<ILancamentoFinanceiroRepository, LancamentoFinanceiroRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
