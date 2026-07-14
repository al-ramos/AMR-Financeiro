using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class ConciliacaoRepository(FinanceiroDbContext ctx) : IConciliacaoRepository
{
    public async Task AddExtratoAsync(ExtratoBancario extrato, CancellationToken ct = default) =>
        await ctx.ExtratosBancarios.AddAsync(extrato, ct);

    public Task<ExtratoBancario?> GetExtratoByIdAsync(int extratoId, CancellationToken ct = default) =>
        ctx.ExtratosBancarios.FirstOrDefaultAsync(e => e.Id == extratoId, ct);

    public async Task<List<MovimentacaoBancaria>> GetMovimentacoesByExtratoAsync(int extratoId, CancellationToken ct = default) =>
        await ctx.MovimentacoesBancarias
            .Where(m => m.ExtratoId == extratoId)
            .OrderBy(m => m.DataLancamento)
            .ThenBy(m => m.Id)
            .ToListAsync(ct);

    public Task<MovimentacaoBancaria?> GetMovimentacaoByIdAsync(int id, CancellationToken ct = default) =>
        ctx.MovimentacoesBancarias.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddMovimentacaoAsync(MovimentacaoBancaria mov, CancellationToken ct = default) =>
        await ctx.MovimentacoesBancarias.AddAsync(mov, ct);

    public Task UpdateMovimentacaoAsync(MovimentacaoBancaria mov, CancellationToken ct = default)
    {
        ctx.MovimentacoesBancarias.Update(mov);
        return Task.CompletedTask;
    }

    public async Task<List<MovimentacaoBancaria>> GetPendentesAsync(int cdFilial, DateOnly dataMinima, CancellationToken ct = default) =>
        await (from m in ctx.MovimentacoesBancarias
               join e in ctx.ExtratosBancarios on m.ExtratoId equals e.Id
               where e.CdFilial == cdFilial
                  && m.StatusConciliacao == StatusConciliacao.Pendente
                  && m.DataLancamento >= dataMinima
               orderby m.DataLancamento, m.Id
               select m)
            .ToListAsync(ct);

    public Task<bool> ExtratoJaImportadoAsync(int cdFilial, string hashArquivo, CancellationToken ct = default) =>
        ctx.Set<ExtratoHash>().AnyAsync(h => h.CdFilial == cdFilial && h.Hash == hashArquivo, ct);

    public async Task SalvarHashAsync(int cdFilial, string hashArquivo, int extratoId, CancellationToken ct = default) =>
        await ctx.Set<ExtratoHash>().AddAsync(new ExtratoHash
        {
            CdFilial = cdFilial,
            Hash = hashArquivo,
            ExtratoId = extratoId,
            ImportadoEm = DateTime.UtcNow
        }, ct);
}

/// <summary>
/// Registro de idempotência da importação de extratos (tabela extratos_hashes).
/// Entidade de infraestrutura — não faz parte do domínio.
/// </summary>
public sealed class ExtratoHash
{
    public int Id { get; set; }
    public int CdFilial { get; set; }
    public string Hash { get; set; } = string.Empty;
    public int ExtratoId { get; set; }
    public DateTime ImportadoEm { get; set; } = DateTime.UtcNow;
}
