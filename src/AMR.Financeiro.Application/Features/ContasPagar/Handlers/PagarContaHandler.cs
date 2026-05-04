using MediatR;
using AMR.Financeiro.Application.Features.ContasPagar.Commands;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasPagar.Handlers;

public class PagarContaHandler(IContaPagarRepository repo, IUnitOfWork uow)
    : IRequestHandler<PagarContaCommand, bool>
{
    public async Task<bool> Handle(PagarContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;
        conta.Pagar(cmd.DataPagamento);
        repo.Atualizar(conta);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
