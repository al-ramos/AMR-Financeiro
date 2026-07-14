using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class PlanoDeContasRepository(FinanceiroDbContext ctx) : IPlanoDeContasRepository
{
    public async Task<List<PlanoDeContas>> GetByCdFilialAsync(int cdFilial, bool incluirInativos = false, CancellationToken ct = default)
    {
        var query = ctx.PlanoDeContas.Where(x => x.CdFilial == cdFilial);

        if (!incluirInativos)
            query = query.Where(x => x.Ativo);

        return await query
            .OrderBy(x => x.OrdemExibicao)
            .ThenBy(x => x.Codigo)
            .ToListAsync(ct);
    }

    public Task<PlanoDeContas?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ctx.PlanoDeContas.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PlanoDeContas?> GetByCodigoAsync(int cdFilial, string codigo, CancellationToken ct = default) =>
        ctx.PlanoDeContas.FirstOrDefaultAsync(x => x.CdFilial == cdFilial && x.Codigo == codigo, ct);

    /// <summary>
    /// Verifica se a conta possui lançamentos financeiros.
    /// Não há coluna ContaId em ContasReceber: o razão do sistema é a tabela Lancamentos
    /// (LancamentoFinanceiro), que referencia o plano legado (PlanoContas) via PlanoContasId.
    /// O vínculo com o plano novo (planodecontas) é feito pelo par (CdFilial, Codigo).
    /// </summary>
    public async Task<bool> TemLancamentosAsync(int contaId, CancellationToken ct = default)
    {
        var conta = await GetByIdAsync(contaId, ct);
        if (conta is null) return false;

        return await ctx.Lancamentos.AnyAsync(l =>
            ctx.PlanoContas.Any(p =>
                p.Id == l.PlanoContasId &&
                p.CdFilial == conta.CdFilial &&
                p.Codigo == conta.Codigo), ct);
    }

    public Task<bool> TemContasFilhasAsync(int contaId, CancellationToken ct = default) =>
        ctx.PlanoDeContas.AnyAsync(x => x.PaiId == contaId, ct);

    public async Task AddAsync(PlanoDeContas conta, CancellationToken ct = default) =>
        await ctx.PlanoDeContas.AddAsync(conta, ct);

    public Task UpdateAsync(PlanoDeContas conta, CancellationToken ct = default)
    {
        ctx.PlanoDeContas.Update(conta);
        return Task.CompletedTask;
    }
}
