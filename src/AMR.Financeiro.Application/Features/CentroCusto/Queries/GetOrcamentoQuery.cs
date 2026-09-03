using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Queries;

public record GetOrcamentoQuery(int CentroCustoId, int Ano) : IRequest<OrcamentoAnualDto>;

public record OrcamentoPorMesDto(int Mes, string NomeMes, decimal Orcado, decimal Realizado,
    decimal PercentualConsumido, bool EmAlerta, bool Estourado);

public record OrcamentoAnualDto(int CentroCustoId, int Ano, List<OrcamentoPorMesDto> Meses,
    decimal TotalOrcado, decimal TotalRealizado);

public class GetOrcamentoHandler(ICentroCustoRepository repo)
    : IRequestHandler<GetOrcamentoQuery, OrcamentoAnualDto>
{
    private static readonly string[] NomesMeses =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    public async Task<OrcamentoAnualDto> Handle(GetOrcamentoQuery query, CancellationToken ct)
    {
        var orcamentos = await repo.GetOrcamentoAsync(query.CentroCustoId, query.Ano, ct);

        var meses = new List<OrcamentoPorMesDto>(12);

        for (var mes = 1; mes <= 12; mes++)
        {
            // Agrega todas as contas do mês; meses sem registro ficam zerados
            var doMes = orcamentos.Where(o => o.Mes == mes).ToList();
            var orcado = doMes.Sum(o => o.ValorOrcado);
            var realizado = doMes.Sum(o => o.ValorRealizado);
            var percentual = orcado == 0 ? 0 : realizado / orcado * 100;

            meses.Add(new OrcamentoPorMesDto(
                mes, NomesMeses[mes - 1], orcado, realizado,
                percentual, percentual >= 90, percentual > 100));
        }

        return new OrcamentoAnualDto(
            query.CentroCustoId, query.Ano, meses,
            meses.Sum(m => m.Orcado), meses.Sum(m => m.Realizado));
    }
}
