using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Queries;

public record GetAlertasQuery(int CdFilial) : IRequest<List<AlertaDto>>;

public record AlertaDto(int CentroCustoId, string CentroCustoNome, string Responsavel,
    string ContaDescricao, int Mes, int Ano, decimal Orcado, decimal Realizado,
    decimal PercentualConsumido, bool Estourado);

public class GetAlertasHandler(ICentroCustoRepository repo)
    : IRequestHandler<GetAlertasQuery, List<AlertaDto>>
{
    public async Task<List<AlertaDto>> Handle(GetAlertasQuery query, CancellationToken ct)
    {
        var alertas = await repo.GetAlertasAsync(query.CdFilial, ct);
        var centros = await repo.GetByCdFilialAsync(query.CdFilial, ct);
        var centrosPorId = centros.ToDictionary(c => c.Id);

        return alertas
            .Select(o =>
            {
                var cc = centrosPorId.GetValueOrDefault(o.CentroCustoId);
                return new AlertaDto(
                    o.CentroCustoId,
                    cc?.Descricao ?? string.Empty,
                    cc?.ResponsavelNome ?? string.Empty,
                    o.ContaDescricao, o.Mes, o.Ano,
                    o.ValorOrcado, o.ValorRealizado,
                    o.PercentualConsumido, o.Estourado);
            })
            .OrderByDescending(a => a.PercentualConsumido)
            .ToList();
    }
}
