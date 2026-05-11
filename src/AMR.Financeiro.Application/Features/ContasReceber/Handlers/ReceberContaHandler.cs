using MediatR;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasReceber.Handlers;

public class ReceberContaHandler(
    IContaReceberRepository repo,
    IPlanoContasRepository planoContasRepo,
    ILancamentoFinanceiroRepository lancamentoRepo,
    IUnitOfWork uow)
    : IRequestHandler<ReceberContaCommand, bool>
{
    // Código do plano de contas padrão para baixa de CR (1.1.3 — Contas a Receber)
    private const string CodigoContasReceber = "1.1.3";

    public async Task<bool> Handle(ReceberContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Receber(cmd.DataRecebimento, cmd.ValorRecebido);
        repo.Atualizar(conta);

        // Registrar lançamento automático de crédito
        var planoConta = await planoContasRepo.ObterPorCodigoAsync(conta.CdFilial, CodigoContasReceber, ct);
        if (planoConta is not null)
        {
            var lancamento = LancamentoFinanceiro.DeRecebimento(
                conta.CdFilial,
                planoConta.Id,
                conta.ValorRecebido ?? conta.Valor,
                cmd.DataRecebimento,
                conta.Id,
                $"Baixa CR: {conta.Descricao}");
            await lancamentoRepo.AdicionarAsync(lancamento, ct);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
