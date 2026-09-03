using MediatR;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.DRE.Queries;

public record GetDreQuery(int CdFilial, int Ano, int Mes) : IRequest<DreResult>;

public class GetDreHandler(IDreService dreService) : IRequestHandler<GetDreQuery, DreResult>
{
    public Task<DreResult> Handle(GetDreQuery q, CancellationToken ct)
    {
        if (q.Mes is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(q.Mes), q.Mes, "Mês deve estar entre 1 e 12.");

        return dreService.CalcularAsync(q.CdFilial, q.Ano, q.Mes, ct);
    }
}
