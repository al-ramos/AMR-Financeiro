using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Conciliacao.Commands;

public record IgnorarMovimentacaoCommand(
    int MovimentacaoBancariaId,
    string Motivo) : IRequest<bool>;

public class IgnorarMovimentacaoCommandHandler(IConciliacaoRepository repo, IUnitOfWork uow)
    : IRequestHandler<IgnorarMovimentacaoCommand, bool>
{
    public async Task<bool> Handle(IgnorarMovimentacaoCommand cmd, CancellationToken ct)
    {
        var mov = await repo.GetMovimentacaoByIdAsync(cmd.MovimentacaoBancariaId, ct);
        if (mov is null) return false;

        mov.Ignorar(cmd.Motivo);
        await repo.UpdateMovimentacaoAsync(mov, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
