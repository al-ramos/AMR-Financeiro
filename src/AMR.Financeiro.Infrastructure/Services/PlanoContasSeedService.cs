using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Infrastructure.Data;
using AMR.Financeiro.Infrastructure.Data.Seeders;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>Ponte entre o comando de seed (Application) e o PlanoContasSeeder (Infrastructure).</summary>
public class PlanoContasSeedService(FinanceiroDbContext ctx) : IPlanoContasSeedService
{
    public Task<int> SeedAsync(int cdFilial, CancellationToken ct = default) =>
        PlanoContasSeeder.SeedAsync(ctx, cdFilial, ct);
}
