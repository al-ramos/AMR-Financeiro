using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Queries;

public record GetDreCentroCustoQuery(int CentroCustoId, DateOnly DataInicio, DateOnly DataFim)
    : IRequest<DreCentroCustoDto>;

/// <summary>Linha da DRE do CC — valores agregados por conta do plano.</summary>
public record ItemDreCCDto(string ContaCodigo, string ContaDescricao, decimal Valor);

/// <summary>Custo recebido via rateio no período (competência dentro do intervalo).</summary>
public record RateioRecebidoDto(int RegraRateioId, string RegraNome, DateOnly Competencia,
    decimal PercentualAplicado, decimal Valor);

public record DreCentroCustoDto(
    int CentroCustoId,
    string CentroCustoCodigo,
    string CentroCustoDescricao,
    string Periodo,
    List<ItemDreCCDto> Receitas,
    decimal TotalReceitas,
    List<ItemDreCCDto> Despesas,
    decimal TotalDespesas,
    List<RateioRecebidoDto> RateiosRecebidos,
    decimal TotalRateiosRecebidos,
    decimal Resultado);

/// <summary>
/// DRE simplificada por centro de custo (Card 23.5): receitas e despesas vêm dos
/// lançamentos vinculados diretamente ao CC (LancamentoFinanceiro.CentroCustoId);
/// os custos indiretos entram pelos rateios executados para o CC no período.
/// </summary>
public class GetDreCentroCustoHandler(ICentroCustoRepository repo)
    : IRequestHandler<GetDreCentroCustoQuery, DreCentroCustoDto>
{
    public async Task<DreCentroCustoDto> Handle(GetDreCentroCustoQuery query, CancellationToken ct)
    {
        if (query.DataFim < query.DataInicio)
            throw new ArgumentOutOfRangeException(nameof(query.DataFim), query.DataFim,
                "Data final deve ser maior ou igual à data inicial.");

        var cc = await repo.GetByIdAsync(query.CentroCustoId, ct)
            ?? throw new InvalidOperationException(
                $"Centro de custo com Id {query.CentroCustoId} não encontrado.");

        var lancamentos = await repo.GetLancamentosPorCentroCustoAsync(
            query.CentroCustoId, query.DataInicio, query.DataFim, ct);

        var receitas = AgruparPorConta(lancamentos.Where(l => l.Tipo == TipoLancamento.Credito));
        var despesas = AgruparPorConta(lancamentos.Where(l => l.Tipo == TipoLancamento.Debito));

        var rateios = await repo.GetRateiosPorCentroCustoAsync(
            query.CentroCustoId, query.DataInicio, query.DataFim, ct);

        var regras = rateios.Count == 0
            ? []
            : await repo.GetRegrasPorIdsAsync(rateios.Select(r => r.RegraRateioId).Distinct().ToList(), ct);
        var nomesRegras = regras.ToDictionary(r => r.Id, r => r.Nome);

        var rateiosRecebidos = rateios
            .Select(r => new RateioRecebidoDto(
                r.RegraRateioId,
                nomesRegras.GetValueOrDefault(r.RegraRateioId, $"Regra #{r.RegraRateioId}"),
                r.Competencia, r.PercentualAplicado, r.ValorRateado))
            .ToList();

        var totalReceitas = receitas.Sum(r => r.Valor);
        var totalDespesas = despesas.Sum(d => d.Valor);
        var totalRateios = rateiosRecebidos.Sum(r => r.Valor);

        return new DreCentroCustoDto(
            cc.Id, cc.Codigo, cc.Descricao,
            $"{query.DataInicio:dd/MM/yyyy} a {query.DataFim:dd/MM/yyyy}",
            receitas, totalReceitas,
            despesas, totalDespesas,
            rateiosRecebidos, totalRateios,
            totalReceitas - totalDespesas - totalRateios);
    }

    private static List<ItemDreCCDto> AgruparPorConta(IEnumerable<Domain.Entities.LancamentoFinanceiro> lancamentos) =>
        lancamentos
            .GroupBy(l => new { l.PlanoContas.Codigo, l.PlanoContas.Descricao })
            .Select(g => new ItemDreCCDto(g.Key.Codigo, g.Key.Descricao, g.Sum(l => l.Valor)))
            .OrderBy(i => i.ContaCodigo)
            .ToList();
}
