using Microsoft.EntityFrameworkCore;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Interfaces;
using RDS.Financeiro.Infrastructure.Data;

namespace RDS.Financeiro.Infrastructure.Repositories;

public class ContaPagarRepository(FinanceiroDbContext ctx) : IContaPagarRepository
{
    public Task<ContaPagar?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        ctx.ContasPagar.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IEnumerable<ContaPagar>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default) =>
        await ctx.ContasPagar.Where(x => x.CdFilial == cdFilial).ToListAsync(ct);

    public async Task AdicionarAsync(ContaPagar conta, CancellationToken ct = default) =>
        await ctx.ContasPagar.AddAsync(conta, ct);

    public void Atualizar(ContaPagar conta) =>
        ctx.ContasPagar.Update(conta);
}
