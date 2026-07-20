using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

public record ReativarContaCommand(int Id) : IRequest<bool>;

public class ReativarContaHandler(IPlanoDeContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<ReativarContaCommand, bool>
{
    public async Task<bool> Handle(ReativarContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.GetByIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Reativar();
        await repo.UpdateAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
