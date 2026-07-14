using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Conciliacao.Commands;

public record ConciliarManualCommand(
    int MovimentacaoBancariaId,
    int LancamentoId,
    string UsuarioId) : IRequest<bool>;

public class ConciliarManualCommandHandler(IConciliacaoRepository repo, IUnitOfWork uow)
    : IRequestHandler<ConciliarManualCommand, bool>
{
    public async Task<bool> Handle(ConciliarManualCommand cmd, CancellationToken ct)
    {
        var mov = await repo.GetMovimentacaoByIdAsync(cmd.MovimentacaoBancariaId, ct);
        if (mov is null) return false;

        mov.ConciliarCom(cmd.LancamentoId, cmd.UsuarioId);
        await repo.UpdateMovimentacaoAsync(mov, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
