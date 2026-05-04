using MediatR;
using AMR.Financeiro.Application.Features.PlanoContas.Commands;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoContas.Handlers;

public class CriarPlanoContasHandler(IPlanoContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<CriarPlanoContasCommand, int>
{
    public async Task<int> Handle(CriarPlanoContasCommand cmd, CancellationToken ct)
    {
        // Valida se código já existe na filial
        var existente = await repo.ObterPorCodigoAsync(cmd.CdFilial, cmd.Codigo, ct);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe uma conta com o código '{cmd.Codigo}' para esta filial.");

        // Valida pai: se informado, deve existir e ser Sintética
        if (cmd.PaiId.HasValue)
        {
            var pai = await repo.ObterPorIdAsync(cmd.PaiId.Value, ct);
            if (pai is null)
                throw new InvalidOperationException($"Conta pai com Id {cmd.PaiId} não encontrada.");
        }

        var conta = new Domain.Entities.PlanoContas(
            cmd.CdFilial, cmd.Codigo, cmd.Descricao, cmd.Tipo, cmd.PaiId);

        await repo.AdicionarAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return conta.Id;
    }
}
