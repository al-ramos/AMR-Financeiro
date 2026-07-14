using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

public record AtualizarContaCommand(int Id, string Descricao, GrupoDRE GrupoDre, int OrdemExibicao) : IRequest<bool>;

public class AtualizarContaHandler(IPlanoDeContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<AtualizarContaCommand, bool>
{
    public async Task<bool> Handle(AtualizarContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.GetByIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Atualizar(cmd.Descricao, cmd.GrupoDre, cmd.OrdemExibicao);
        await repo.UpdateAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
