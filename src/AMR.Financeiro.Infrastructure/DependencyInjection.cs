using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;
using AMR.Financeiro.Infrastructure.Repositories;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Infrastructure.Messaging;
using AMR.Financeiro.Infrastructure.Parsers;
using AMR.Financeiro.Infrastructure.Services;

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

            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IContaPagarRepository, ContaPagarRepository>();
        services.AddScoped<IContaReceberRepository, ContaReceberRepository>();
        services.AddScoped<ILancamentoFinanceiroRepository, LancamentoFinanceiroRepository>();
        services.AddScoped<IPlanoContasRepository, PlanoContasRepository>();
        services.AddScoped<IPlanoDeContasRepository, PlanoDeContasRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<INFeRepository, NFeRepository>();
        services.AddScoped<IBoletoRepository, BoletoRepository>();
        services.AddScoped<IConciliacaoRepository, ConciliacaoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<INFeService, NFeService>();
        services.AddScoped<IBoletoService, BoletoService>();
        services.AddScoped<IConciliacaoMatchingService, ConciliacaoMatchingService>();
        services.AddScoped<IDreService, DreService>();
        services.AddScoped<IPlanoContasSeedService, PlanoContasSeedService>();

        // Parsers de extrato — o handler recebe IEnumerable<IExtratoParser> e escolhe via Suporta()
        services.AddScoped<IExtratoParser, OfxParser>();
        services.AddScoped<IExtratoParser, Cnab240ExtratoParser>();

        services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

        return services;
    }
}