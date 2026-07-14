using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Queries;

public record GetCentrosCustoQuery(int CdFilial) : IRequest<List<CentroCustoDto>>;

public record CentroCustoDto(int Id, string Codigo, string Descricao, TipoCentroCusto Tipo,
    int? PaiId, int Nivel, string ResponsavelNome, bool Ativo);

public class GetCentrosCustoHandler(ICentroCustoRepository repo)
    : IRequestHandler<GetCentrosCustoQuery, List<CentroCustoDto>>
{
    public async Task<List<CentroCustoDto>> Handle(GetCentrosCustoQuery query, CancellationToken ct)
    {
        var centros = await repo.GetByCdFilialAsync(query.CdFilial, ct);

        return centros
            .Select(c => new CentroCustoDto(c.Id, c.Codigo, c.Descricao, c.Tipo,
                c.PaiId, c.Nivel, c.ResponsavelNome, c.Ativo))
            .ToList();
    }
}
