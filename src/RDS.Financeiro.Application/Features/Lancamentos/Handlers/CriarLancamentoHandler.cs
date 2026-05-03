using MediatR;
using RDS.Financeiro.Application.Features.Lancamentos.Commands;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Enums;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Application.Features.Lancamentos.Handlers;

public class CriarLancamentoHandler(
    ILancamentoFinanceiroRepository repo,
    IPlanoContasRepository planoRepo,
    IUnitOfWork uow)
    : IRequestHandler<CriarLancamentoCommand, int>
{
    public async Task<int> Handle(CriarLancamentoCommand cmd, CancellationToken ct)
    {
        if (cmd.Valor <= 0)
            throw new InvalidOperationException("O valor do lançamento deve ser maior que zero.");

        var plano = await planoRepo.ObterPorIdAsync(cmd.PlanoContasId, ct)
            ?? throw new InvalidOperationException($"Plano de Contas Id {cmd.PlanoContasId} não encontrado.");

        if (!plano.AceitaLancamentos())
            throw new InvalidOperationException(
                $"A conta '{plano.Codigo} — {plano.Descricao}' é Sintética e não aceita lançamentos diretos. Use uma conta Analítica.");

        if (!plano.Ativo)
            throw new InvalidOperationException(
                $"A conta '{plano.Codigo} — {plano.Descricao}' está inativa.");

        var lancamento = new LancamentoFinanceiro(
            cmd.CdFilial,
            cmd.PlanoContasId,
            cmd.Tipo,
            OrigemLancamento.Manual,
            cmd.Valor,
            cmd.DataLancamento,
            cmd.Historico,
            cmd.DocumentoOrigemId);

        await repo.AdicionarAsync(lancamento, ct);
        await uow.SaveChangesAsync(ct);
        return lancamento.Id;
    }
}
