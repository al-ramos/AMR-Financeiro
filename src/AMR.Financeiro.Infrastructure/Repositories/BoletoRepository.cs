using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class BoletoRepository(FinanceiroDbContext ctx) : IBoletoRepository
{
    public Task<Boleto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ctx.Boletos.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Boleto>> GetByCdFilialAsync(int cdFilial, StatusBoleto? status, BancoBoleto? banco, CancellationToken ct = default)
    {
        var query = ctx.Boletos.Where(x => x.CdFilial == cdFilial);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (banco.HasValue)
            query = query.Where(x => x.Banco == banco.Value);

        return await query
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync(ct);
    }

    public Task<Boleto?> GetByNossoNumeroAsync(string nossoNumero, BancoBoleto banco, CancellationToken ct = default) =>
        ctx.Boletos.FirstOrDefaultAsync(x => x.NossoNumero == nossoNumero && x.Banco == banco, ct);

    public async Task<int> GetNextNossoNumeroAsync(BancoBoleto banco, int cdFilial, CancellationToken ct = default)
    {
        // NossoNumero é zero-padded com largura fixa por banco (corpo + 1 dígito verificador),
        // então MAX lexicográfico equivale ao MAX numérico dentro do mesmo banco/filial.
        var maxNossoNumero = await ctx.Boletos
            .Where(x => x.Banco == banco && x.CdFilial == cdFilial)
            .MaxAsync(x => (string?)x.NossoNumero, ct);

        if (string.IsNullOrEmpty(maxNossoNumero))
            return 1;

        // Remove o dígito verificador final para recuperar o sequencial.
        var corpo = maxNossoNumero[..^1];
        return int.TryParse(corpo, out var atual) ? atual + 1 : 1;
    }

    // AddAsync/UpdateAsync persistem imediatamente (SaveChanges) — mesmo racional do
    // NFeRepository: o fluxo de geração precisa do Id gravado entre as etapas
    // (gerado → remessa → registrado → retorno/pago).
    public async Task AddAsync(Boleto boleto, CancellationToken ct = default)
    {
        await ctx.Boletos.AddAsync(boleto, ct);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Boleto boleto, CancellationToken ct = default)
    {
        ctx.Boletos.Update(boleto);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task AddRemessaAsync(RemessaBancaria remessa, CancellationToken ct = default)
    {
        await ctx.RemessasBancarias.AddAsync(remessa, ct);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task AddRetornoAsync(RetornoBancario retorno, CancellationToken ct = default)
    {
        await ctx.RetornosBancarios.AddAsync(retorno, ct);
        await ctx.SaveChangesAsync(ct);
    }
}
