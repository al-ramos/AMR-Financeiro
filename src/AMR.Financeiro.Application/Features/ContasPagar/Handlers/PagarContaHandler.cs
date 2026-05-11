using MediatR;
using AMR.Financeiro.Application.Features.ContasPagar.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasPagar.Handlers;

public class PagarContaHandler(
    IContaPagarRepository repo,
    IPlanoContasRepository planoContasRepo,
    ILancamentoFinanceiroRepository lancamentoRepo,
    IUnitOfWork uow)
    : IRequestHandler<PagarContaCommand, bool>
{
    // Código do plano de contas padrão para baixa de CP (2.1.2 — Contas a Pagar)
    private const string CodigoContasPagar = "2.1.2";

    public async Task<bool> Handle(PagarContaCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Pagar(cmd.DataPagamento);
        repo.Atualizar(conta);

        // Registrar lançamento automático de débito
        var planoConta = await planoContasRepo.ObterPorCodigoAsync(conta.CdFilial, CodigoContasPagar, ct);
        if (planoConta is not null)
        {
            var lancamento = LancamentoFinanceiro.DePagamento(
                conta.CdFilial,
                planoConta.Id,
                conta.Valor,
                cmd.DataPagamento,
                conta.Id,
                $"Baixa CP: {conta.Descricao}");
            await lancamentoRepo.AdicionarAsync(lancamento, ct);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
