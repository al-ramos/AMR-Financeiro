using MediatR;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasReceber.Handlers;

public class CriarContaReceberHandler(IContaReceberRepository repo, IUnitOfWork uow)
    : IRequestHandler<CriarContaReceberCommand, int>
{
    public async Task<int> Handle(CriarContaReceberCommand cmd, CancellationToken ct)
    {
        var conta = new ContaReceber(cmd.CdFilial, cmd.Descricao, cmd.Valor, cmd.Vencimento, cmd.DocumentoOrigem);
        await repo.AdicionarAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return conta.Id;
    }
}
