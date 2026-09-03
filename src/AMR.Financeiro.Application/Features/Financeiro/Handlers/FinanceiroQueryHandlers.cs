using MediatR;
using AMR.Financeiro.Application.Features.Financeiro.Dtos;
using AMR.Financeiro.Application.Features.Financeiro.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Financeiro.Handlers;

public class GetAgingHandler(IParcelamentoRepository repo) : IRequestHandler<GetAgingQuery, AgingDto>
{
    public async Task<AgingDto> Handle(GetAgingQuery req, CancellationToken ct)
    {
        var hoje = DateTime.UtcNow.Date;
        var abertas = await repo.ObterParcelasEmAbertoAsync(ct);

        AgingFaixaDto Faixa(string nome, IEnumerable<Parcela> items)
        {
            var list = items.ToList();
            return new(nome, list.Count, list.Sum(x => x.ValorParcela));
        }

        int DaysLate(Parcela p) => (hoje - p.DataVencimento.Date).Days;

        return new AgingDto(
            Faixa("A vencer",       abertas.Where(p => DaysLate(p) < 0)),
            Faixa("1 a 30 dias",    abertas.Where(p => DaysLate(p) is >= 0 and <= 30)),
            Faixa("31 a 60 dias",   abertas.Where(p => DaysLate(p) is >= 31 and <= 60)),
            Faixa("61 a 90 dias",   abertas.Where(p => DaysLate(p) is >= 61 and <= 90)),
            Faixa("Acima 90 dias",  abertas.Where(p => DaysLate(p) > 90)),
            abertas.Sum(p => p.ValorParcela)
        );
    }
}

public class GetFluxoCaixaHandler(
    IParcelamentoRepository parcelamentoRepo,
    ILancamentoFinanceiroRepository lancamentoRepo)
    : IRequestHandler<GetFluxoCaixaQuery, FluxoCaixaDto>
{
    public async Task<FluxoCaixaDto> Handle(GetFluxoCaixaQuery req, CancellationToken ct)
    {
        var horizonte = req.HorizonteDias is 30 or 60 or 90 ? req.HorizonteDias : 30;
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var fim = hoje.AddDays(horizonte);

        var parcelas = (await parcelamentoRepo.ObterParcelasEmAbertoAsync(ct))
            .Where(p => p.EstaEmAberto)
            .Where(p => DateOnly.FromDateTime(p.DataVencimento) >= hoje
                     && DateOnly.FromDateTime(p.DataVencimento) <= fim)
            .ToList();

        var lancamentos = await lancamentoRepo.ObterFuturosAsync(hoje, fim, ct);

        var dias = new Dictionary<DateTime, (decimal Entradas, decimal Saidas)>();

        foreach (var p in parcelas)
        {
            var d = p.DataVencimento.Date;
            dias.TryAdd(d, (0, 0));
            dias[d] = (dias[d].Entradas, dias[d].Saidas + p.ValorParcela);
        }

        foreach (var l in lancamentos)
        {
            var d = l.DataLancamento.ToDateTime(TimeOnly.MinValue);
            dias.TryAdd(d, (0, 0));
            if (l.Tipo == TipoLancamento.Credito)
                dias[d] = (dias[d].Entradas + l.Valor, dias[d].Saidas);
            else
                dias[d] = (dias[d].Entradas, dias[d].Saidas + l.Valor);
        }

        var resultado = dias
            .OrderBy(kv => kv.Key)
            .Select(kv => new FluxoCaixaDiaDto(
                kv.Key,
                kv.Value.Entradas,
                kv.Value.Saidas,
                kv.Value.Entradas - kv.Value.Saidas))
            .ToList();

        return new FluxoCaixaDto(
            horizonte,
            resultado,
            resultado.Sum(d => d.Entradas),
            resultado.Sum(d => d.Saidas),
            resultado.Sum(d => d.Saldo)
        );
    }
}
