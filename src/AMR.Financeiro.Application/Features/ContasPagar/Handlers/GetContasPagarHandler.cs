using MediatR;
using AMR.Financeiro.Application.Features.ContasPagar.Dtos;
using AMR.Financeiro.Application.Features.ContasPagar.Queries;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasPagar.Handlers;

public class GetContasPagarHandler(IContaPagarRepository repo)
    : IRequestHandler<GetContasPagarQuery, IEnumerable<ContaPagarDto>>
{
    public async Task<IEnumerable<ContaPagarDto>> Handle(GetContasPagarQuery q, CancellationToken ct)
    {
        var contas = await repo.ObterPorFilialAsync(q.CdFilial, ct);
        return contas.Select(c => new ContaPagarDto(
            c.Id, c.CdFilial, c.Descricao, c.Valor,
            c.Vencimento, c.DataPagamento, c.Status.ToString()));
    }
}
