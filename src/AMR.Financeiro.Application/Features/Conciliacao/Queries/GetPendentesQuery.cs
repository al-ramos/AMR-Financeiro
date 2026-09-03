using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Conciliacao.Queries;

public record GetPendentesQuery(int CdFilial, int DiasPassados = 60) : IRequest<List<MovimentacaoDto>>;

public class GetPendentesQueryHandler(IConciliacaoRepository repo)
    : IRequestHandler<GetPendentesQuery, List<MovimentacaoDto>>
{
    public async Task<List<MovimentacaoDto>> Handle(GetPendentesQuery query, CancellationToken ct)
    {
        var dataMinima = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-query.DiasPassados);
        var pendentes = await repo.GetPendentesAsync(query.CdFilial, dataMinima, ct);

        return pendentes
            .Select(m => new MovimentacaoDto(
                m.Id, m.DataLancamento, m.Tipo, m.Valor, m.Descricao, m.CodigoDoc,
                m.StatusConciliacao, m.LancamentoId, m.ConciliadoPor, m.ConciliadoEm))
            .ToList();
    }
}
