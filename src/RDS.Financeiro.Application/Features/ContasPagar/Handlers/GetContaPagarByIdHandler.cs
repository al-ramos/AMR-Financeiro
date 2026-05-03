using MediatR;
using RDS.Financeiro.Application.Features.ContasPagar.Dtos;
using RDS.Financeiro.Application.Features.ContasPagar.Queries;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Application.Features.ContasPagar.Handlers;

public class GetContaPagarByIdHandler(IContaPagarRepository repo)
    : IRequestHandler<GetContaPagarByIdQuery, ContaPagarDto?>
{
    public async Task<ContaPagarDto?> Handle(GetContaPagarByIdQuery q, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(q.Id, ct);
        if (conta is null) return null;

        return new ContaPagarDto(
            conta.Id,
            conta.CdFilial,
            conta.Descricao,
            conta.Valor,
            conta.Vencimento,
            conta.DataPagamento,
            conta.Status.ToString());
    }
}
