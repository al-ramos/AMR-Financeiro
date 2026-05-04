using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class LancamentoFinanceiroRepository(FinanceiroDbContext ctx) : ILancamentoFinanceiroRepository
{
    public Task<LancamentoFinanceiro?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        ctx.Lancamentos.Include(x => x.PlanoContas).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IEnumerable<LancamentoFinanceiro>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default) =>
        await ctx.Lancamentos
            .Include(x => x.PlanoContas)
            .Where(x => x.CdFilial == cdFilial)
            .OrderByDescending(x => x.DataLancamento)
            .ToListAsync(ct);

    public async Task<IEnumerable<LancamentoFinanceiro>> ObterPorPlanoContasAsync(int planoContasId, CancellationToken ct = default) =>
        await ctx.Lancamentos
            .Where(x => x.PlanoContasId == planoContasId)
            .OrderByDescending(x => x.DataLancamento)
            .ToListAsync(ct);

    public async Task<IEnumerable<LancamentoFinanceiro>> ObterPorPeriodoAsync(int cdFilial, DateOnly inicio, DateOnly fim, CancellationToken ct = default) =>
        await ctx.Lancamentos
            .Include(x => x.PlanoContas)
            .Where(x => x.CdFilial == cdFilial
                     && x.DataLancamento >= inicio
                     && x.DataLancamento <= fim)
            .OrderByDescending(x => x.DataLancamento)
            .ToListAsync(ct);

    public async Task AdicionarAsync(LancamentoFinanceiro lancamento, CancellationToken ct = default) =>
        await ctx.Lancamentos.AddAsync(lancamento, ct);
}
