using MediatR;
using RDS.Financeiro.Application.Features.ContasPagar.Commands;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Application.Features.ContasPagar.Handlers;

public class CancelarContaHandler(IContaPagarRepository repo, IUnitOfWork uow)
    : IRequestHandler<CancelarContaCommand, bool>
{
    public async Task<bool> Handle(CancelarContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Cancelar();
        repo.Atualizar(conta);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
