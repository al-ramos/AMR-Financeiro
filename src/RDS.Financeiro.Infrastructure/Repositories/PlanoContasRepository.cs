using Microsoft.EntityFrameworkCore;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Interfaces;
using RDS.Financeiro.Infrastructure.Data;

namespace RDS.Financeiro.Infrastructure.Repositories;

public class PlanoContasRepository(FinanceiroDbContext ctx) : IPlanoContasRepository
{
    public Task<PlanoContas?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        ctx.PlanoContas.Include(x => x.Filhos).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PlanoContas?> ObterPorCodigoAsync(int cdFilial, string codigo, CancellationToken ct = default) =>
        ctx.PlanoContas.FirstOrDefaultAsync(x => x.CdFilial == cdFilial && x.Codigo == codigo, ct);

    public async Task<IEnumerable<PlanoContas>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default) =>
        await ctx.PlanoContas
            .Where(x => x.CdFilial == cdFilial)
            .OrderBy(x => x.Codigo)
            .ToListAsync(ct);

    public async Task<IEnumerable<PlanoContas>> ObterFilhosAsync(int paiId, CancellationToken ct = default) =>
        await ctx.PlanoContas
            .Where(x => x.PaiId == paiId)
            .OrderBy(x => x.Codigo)
            .ToListAsync(ct);

    public async Task AdicionarAsync(PlanoContas conta, CancellationToken ct = default) =>
        await ctx.PlanoContas.AddAsync(conta, ct);

    public void Atualizar(PlanoContas conta) =>
        ctx.PlanoContas.Update(conta);
}
