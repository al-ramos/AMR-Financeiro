using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

public record InativarContaCommand(int Id) : IRequest<bool>;

public class InativarContaHandler(IPlanoDeContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<InativarContaCommand, bool>
{
    public async Task<bool> Handle(InativarContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.GetByIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        if (await repo.TemLancamentosAsync(cmd.Id, ct))
            throw new InvalidOperationException("Conta possui lançamentos e não pode ser inativada.");

        if (await repo.TemContasFilhasAsync(cmd.Id, ct))
            throw new InvalidOperationException("Conta possui contas filhas e não pode ser inativada.");

        conta.Inativar();
        await repo.UpdateAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
