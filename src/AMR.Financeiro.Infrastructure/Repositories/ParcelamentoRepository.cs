using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class ParcelamentoRepository(FinanceiroDbContext ctx) : IParcelamentoRepository
{
    public Task<Parcelamento?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        ctx.Parcelamentos
            .Include(x => x.Parcelas)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Parcelamento>> ListarAsync(CancellationToken ct = default) =>
        await ctx.Parcelamentos
            .Include(x => x.Parcelas)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<Parcela>> ObterParcelasEmAbertoAsync(CancellationToken ct = default) =>
        await ctx.Parcelas
            .Where(p => p.Status == StatusParcela.Pendente || p.Status == StatusParcela.Vencido)
            .OrderBy(p => p.DataVencimento)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Parcelamento parcelamento, CancellationToken ct = default) =>
        await ctx.Parcelamentos.AddAsync(parcelamento, ct);

    public Task AtualizarAsync(Parcelamento parcelamento, CancellationToken ct = default)
    {
        ctx.Parcelamentos.Update(parcelamento);
        return Task.CompletedTask;
    }
}
