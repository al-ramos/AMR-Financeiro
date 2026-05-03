using MediatR;
using RDS.Financeiro.Application.Features.ContasPagar.Commands;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Application.Features.ContasPagar.Handlers;

public class CriarContaPagarHandler(IContaPagarRepository repo, IUnitOfWork uow)
    : IRequestHandler<CriarContaPagarCommand, int>
{
    public async Task<int> Handle(CriarContaPagarCommand cmd, CancellationToken ct)
    {
        var conta = new ContaPagar(cmd.CdFilial, cmd.Descricao, cmd.Valor, cmd.Vencimento);
        await repo.AdicionarAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return conta.Id;
    }
}
