using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class NFeRepository(FinanceiroDbContext ctx) : INFeRepository
{
    public Task<NotaFiscal?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ctx.NotasFiscais.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<NotaFiscal?> GetByChaveAcessoAsync(string chaveAcesso, CancellationToken ct = default) =>
        ctx.NotasFiscais.FirstOrDefaultAsync(x => x.ChaveAcesso == chaveAcesso, ct);

    public async Task<IReadOnlyList<NotaFiscal>> GetByCdFilialAsync(int cdFilial, int? ano = null, CancellationToken ct = default)
    {
        var query = ctx.NotasFiscais.Where(x => x.CdFilial == cdFilial);

        if (ano.HasValue)
            query = query.Where(x => x.CriadoEm.Year == ano.Value);

        return await query
            .OrderByDescending(x => x.NumeroNF)
            .ToListAsync(ct);
    }

    // AddAsync/UpdateAsync persistem imediatamente (SaveChanges) porque o fluxo de
    // emissão precisa do Id gerado e do estado gravado entre as etapas
    // (digitada → envio SEFAZ → autorizada/rejeitada), diferente dos demais
    // repositórios que delegam o commit ao IUnitOfWork.
    public async Task AddAsync(NotaFiscal nfe, CancellationToken ct = default)
    {
        await ctx.NotasFiscais.AddAsync(nfe, ct);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NotaFiscal nfe, CancellationToken ct = default)
    {
        ctx.NotasFiscais.Update(nfe);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task<long> GetNextNumeroNFAsync(int cdFilial, ModeloNFe modelo, int serie, CancellationToken ct = default)
    {
        var ultimo = await ctx.NotasFiscais
            .Where(x => x.CdFilial == cdFilial && x.Modelo == modelo && x.Serie == serie)
            .MaxAsync(x => (long?)x.NumeroNF, ct);

        return (ultimo ?? 0) + 1;
    }
}
