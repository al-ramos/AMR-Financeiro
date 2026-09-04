using AMR.Financeiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

/// <summary>
/// Repositório de centros de custo, orçamentos e rateios (Card 23.5).
/// Observação: os métodos de escrita persistem imediatamente (SaveChangesAsync interno)
/// porque o contrato também é consumido pelo RateioService, que não recebe IUnitOfWork.
/// </summary>
public class CentroCustoRepository(FinanceiroDbContext ctx) : ICentroCustoRepository
{
    public async Task<List<CentroCusto>> GetByCdFilialAsync(int cdFilial, CancellationToken ct = default) =>
        await ctx.CentrosCusto
            .Where(c => c.CdFilial == cdFilial)
            .OrderBy(c => c.Codigo)
            .ToListAsync(ct);

    public Task<CentroCusto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ctx.CentrosCusto.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(CentroCusto cc, CancellationToken ct = default)
    {
        await ctx.CentrosCusto.AddAsync(cc, ct);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CentroCusto cc, CancellationToken ct = default)
    {
        ctx.CentrosCusto.Update(cc);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task<List<OrcamentoCC>> GetOrcamentoAsync(int centroCustoId, int ano, CancellationToken ct = default) =>
        await ctx.OrcamentosCC
            .Where(o => o.CentroCustoId == centroCustoId && o.Ano == ano)
            .OrderBy(o => o.Mes)
            .ThenBy(o => o.ContaDescricao)
            .ToListAsync(ct);

    public async Task UpsertOrcamentoAsync(OrcamentoCC orcamento, CancellationToken ct = default)
    {
        var existente = await ctx.OrcamentosCC.FirstOrDefaultAsync(o =>
            o.CentroCustoId == orcamento.CentroCustoId &&
            o.ContaDescricao == orcamento.ContaDescricao &&
            o.Ano == orcamento.Ano &&
            o.Mes == orcamento.Mes, ct);

        if (existente is null)
            await ctx.OrcamentosCC.AddAsync(orcamento, ct);
        else
            existente.AtualizarOrcado(orcamento.ValorOrcado); // preserva o realizado

        await ctx.SaveChangesAsync(ct);
    }

    public async Task<List<RegraRateio>> GetRegrasAtivasAsync(int cdFilial, CancellationToken ct = default) =>
        await ctx.RegrasRateio
            .Include(r => r.Destinos)
            .Where(r => r.CdFilial == cdFilial && r.Ativo)
            .OrderBy(r => r.Nome)
            .ToListAsync(ct);

    public async Task AddRegraAsync(RegraRateio regra, List<RegraRateioDestino> destinos, CancellationToken ct = default)
    {
        // Vincula pela navegação: o EF Core preenche RegraRateioId ao inserir
        foreach (var destino in destinos)
            regra.Destinos.Add(destino);

        await ctx.RegrasRateio.AddAsync(regra, ct);
        await ctx.SaveChangesAsync(ct);
    }

    public Task<bool> RateioJaExecutadoAsync(int cdFilial, DateOnly competencia, CancellationToken ct = default) =>
        ctx.RateiosRealizados.AnyAsync(rr =>
            rr.Competencia == competencia &&
            ctx.RegrasRateio.Any(r => r.Id == rr.RegraRateioId && r.CdFilial == cdFilial), ct);

    public async Task AddRateiosAsync(List<RateioRealizado> rateios, CancellationToken ct = default)
    {
        await ctx.RateiosRealizados.AddRangeAsync(rateios, ct);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task<List<LancamentoFinanceiro>> GetLancamentosPorCentroCustoAsync(
        int centroCustoId, DateOnly inicio, DateOnly fim, CancellationToken ct = default) =>
        await ctx.Lancamentos
            .AsNoTracking()
            .Include(l => l.Conta)
            .Where(l => l.CentroCustoId == centroCustoId
                     && l.DataLancamento >= inicio
                     && l.DataLancamento <= fim)
            .OrderBy(l => l.DataLancamento)
            .ToListAsync(ct);

    public async Task<List<RateioRealizado>> GetRateiosPorCentroCustoAsync(
        int centroCustoId, DateOnly inicio, DateOnly fim, CancellationToken ct = default) =>
        await ctx.RateiosRealizados
            .AsNoTracking()
            .Where(rr => rr.CentroCustoId == centroCustoId
                      && rr.Competencia >= inicio
                      && rr.Competencia <= fim)
            .OrderBy(rr => rr.Competencia)
            .ToListAsync(ct);

    public async Task<List<RegraRateio>> GetRegrasPorIdsAsync(List<int> ids, CancellationToken ct = default) =>
        await ctx.RegrasRateio
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .ToListAsync(ct);

    public async Task<List<OrcamentoCC>> GetAlertasAsync(int cdFilial, CancellationToken ct = default) =>
        await (from o in ctx.OrcamentosCC
               join c in ctx.CentrosCusto on o.CentroCustoId equals c.Id
               where c.CdFilial == cdFilial
                  && o.ValorOrcado > 0
                  && o.ValorRealizado >= o.ValorOrcado * 0.9m // EmAlerta (>= 90%) — inclui os estourados
               orderby o.Ano, o.Mes
               select o)
            .ToListAsync(ct);

    // FIN-02 — a base do rateio deixa de ser um valor fixo e passa a sair dos
    // lancamentos da conta de origem na competencia. Mesma convencao da DRE:
    // conta devedora acumula debitos menos creditos, credora o inverso.
    public async Task<decimal?> ObterTotalDaContaAsync(
        int cdFilial, int contaOrigemId, DateOnly competencia, CancellationToken ct = default)
    {
        var conta = await ctx.PlanoDeContas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contaOrigemId && c.CdFilial == cdFilial, ct);

        if (conta is null) return null;

        var inicio = new DateOnly(competencia.Year, competencia.Month, 1);
        var fim = inicio.AddMonths(1).AddDays(-1);

        var somas = await ctx.Lancamentos.AsNoTracking()
            .Where(l => l.CdFilial == cdFilial
                     && l.PlanoContasId == contaOrigemId
                     && l.DataLancamento >= inicio
                     && l.DataLancamento <= fim)
            .GroupBy(l => l.Tipo)
            .Select(g => new { Tipo = g.Key, Total = g.Sum(x => x.Valor) })
            .ToListAsync(ct);

        var creditos = somas.Where(x => x.Tipo == TipoLancamento.Credito).Sum(x => x.Total);
        var debitos = somas.Where(x => x.Tipo == TipoLancamento.Debito).Sum(x => x.Total);

        return conta.Natureza == NaturezaConta.Credora
            ? creditos - debitos
            : debitos - creditos;
    }
}
