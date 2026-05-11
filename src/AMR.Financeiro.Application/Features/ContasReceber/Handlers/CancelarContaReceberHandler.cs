using MediatR;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasReceber.Handlers;

public class CancelarContaReceberHandler(IContaReceberRepository repo, IUnitOfWork uow)
    : IRequestHandler<CancelarContaReceberCommand, bool>
{
    public async Task<bool> Handle(CancelarContaReceberCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Cancelar();
        repo.Atualizar(conta);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
