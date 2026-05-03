using Microsoft.EntityFrameworkCore;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Enums;
using RDS.Financeiro.Domain.Interfaces;
using RDS.Financeiro.Infrastructure.Data;

namespace RDS.Financeiro.Infrastructure.Repositories;

public class ContaReceberRepository(FinanceiroDbContext ctx) : IContaReceberRepository
{
    public Task<ContaReceber?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        ctx.ContasReceber.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IEnumerable<ContaReceber>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default) =>
        await ctx.ContasReceber.Where(x => x.CdFilial == cdFilial).ToListAsync(ct);

    public async Task<IEnumerable<ContaReceber>> ObterVencidasAsync(int cdFilial, CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await ctx.ContasReceber
            .Where(x => x.CdFilial == cdFilial
                     && x.Status == StatusContaReceber.Aberta
                     && x.Vencimento < hoje)
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(ContaReceber conta, CancellationToken ct = default) =>
        await ctx.ContasReceber.AddAsync(conta, ct);

    public void Atualizar(ContaReceber conta) =>
        ctx.ContasReceber.Update(conta);
}
