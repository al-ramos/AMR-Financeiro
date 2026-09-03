using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class ContaBancariaRepository(FinanceiroDbContext ctx) : IContaBancariaRepository
{
    public Task<ContaBancaria?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        ctx.ContasBancarias.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<ContaBancaria>> ListarAsync(bool incluirInativas, CancellationToken ct = default)
    {
        var query = ctx.ContasBancarias.AsQueryable();
        if (!incluirInativas)
            query = query.Where(x => x.Ativa);
        return await query.OrderBy(x => x.Nome).ToListAsync(ct);
    }

    // Saldo nunca é persistido: uma única query com subqueries correlacionadas agrega
    // créditos e débitos dos lançamentos vinculados desde DataSaldoInicial de cada conta.
    public async Task<Dictionary<int, decimal>> ObterSaldosAsync(bool incluirInativas, CancellationToken ct = default)
    {
        var query = ctx.ContasBancarias.AsQueryable();
        if (!incluirInativas)
            query = query.Where(x => x.Ativa);

        var saldos = await query
            .Select(c => new
            {
                c.Id,
                Saldo = c.SaldoInicial
                    + (ctx.Lancamentos
                        .Where(l => l.ContaBancariaId == c.Id
                                 && l.Tipo == TipoLancamento.Credito
                                 && l.DataLancamento >= DateOnly.FromDateTime(c.DataSaldoInicial))
                        .Sum(l => (decimal?)l.Valor) ?? 0m)
                    - (ctx.Lancamentos
                        .Where(l => l.ContaBancariaId == c.Id
                                 && l.Tipo == TipoLancamento.Debito
                                 && l.DataLancamento >= DateOnly.FromDateTime(c.DataSaldoInicial))
                        .Sum(l => (decimal?)l.Valor) ?? 0m),
            })
            .ToListAsync(ct);

        return saldos.ToDictionary(x => x.Id, x => x.Saldo);
    }

    public async Task<decimal?> ObterSaldoAsync(int contaId, CancellationToken ct = default)
    {
        var saldo = await ctx.ContasBancarias
            .Where(c => c.Id == contaId)
            .Select(c => (decimal?)(c.SaldoInicial
                + (ctx.Lancamentos
                    .Where(l => l.ContaBancariaId == c.Id
                             && l.Tipo == TipoLancamento.Credito
                             && l.DataLancamento >= DateOnly.FromDateTime(c.DataSaldoInicial))
                    .Sum(l => (decimal?)l.Valor) ?? 0m)
                - (ctx.Lancamentos
                    .Where(l => l.ContaBancariaId == c.Id
                             && l.Tipo == TipoLancamento.Debito
                             && l.DataLancamento >= DateOnly.FromDateTime(c.DataSaldoInicial))
                    .Sum(l => (decimal?)l.Valor) ?? 0m)))
            .FirstOrDefaultAsync(ct);

        return saldo;
    }

    public async Task<List<LancamentoFinanceiro>> ObterExtratoAsync(int contaId, CancellationToken ct = default) =>
        await ctx.Lancamentos
            .Include(x => x.Conta)
            .Where(x => x.ContaBancariaId == contaId)
            .OrderByDescending(x => x.DataLancamento)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

    public async Task AdicionarAsync(ContaBancaria conta, CancellationToken ct = default) =>
        await ctx.ContasBancarias.AddAsync(conta, ct);

    public Task AtualizarAsync(ContaBancaria conta, CancellationToken ct = default)
    {
        ctx.ContasBancarias.Update(conta);
        return Task.CompletedTask;
    }
}
