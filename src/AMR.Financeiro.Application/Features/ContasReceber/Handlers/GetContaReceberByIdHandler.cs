using MediatR;
using AMR.Financeiro.Application.Features.ContasReceber.Dtos;
using AMR.Financeiro.Application.Features.ContasReceber.Queries;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasReceber.Handlers;

public class GetContaReceberByIdHandler(IContaReceberRepository repo)
    : IRequestHandler<GetContaReceberByIdQuery, ContaReceberDto?>
{
    public async Task<ContaReceberDto?> Handle(GetContaReceberByIdQuery q, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(q.Id, ct);
        if (conta is null) return null;

        return new ContaReceberDto(
            conta.Id, conta.CdFilial, conta.Descricao, conta.Valor,
            conta.Vencimento, conta.DataRecebimento, conta.ValorRecebido,
            conta.DocumentoOrigem, conta.Status.ToString());
    }
}
