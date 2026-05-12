using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Domain.Events;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Application.Features.Lancamentos.Commands;
using MediatR;

namespace AMR.Financeiro.Application.Features.Lancamentos.Handlers;

public class CriarLancamentoHandler(
    ILancamentoFinanceiroRepository repo,
    IPlanoContasRepository planoRepo,
    IUnitOfWork uow,
    IEventPublisher publisher)
    : IRequestHandler<CriarLancamentoCommand, int>
{
    public async Task<int> Handle(CriarLancamentoCommand cmd, CancellationToken ct)
    {
        if (cmd.Valor <= 0)
            throw new InvalidOperationException("O valor do lancamento deve ser maior que zero.");

        var plano = await planoRepo.ObterPorIdAsync(cmd.PlanoContasId, ct)
            ?? throw new InvalidOperationException($"Plano de Contas Id {cmd.PlanoContasId} nao encontrado.");

        if (!plano.AceitaLancamentos())
            throw new InvalidOperationException(
                $"A conta '{plano.Codigo} - {plano.Descricao}' e Sintetica e nao aceita lancamentos diretos. Use uma conta Analitica.");

        if (!plano.Ativo)
            throw new InvalidOperationException(
                $"A conta '{plano.Codigo} - {plano.Descricao}' esta inativa.");

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

        await publisher.PublishAsync(new LancamentoCriadoEvent
        {
            LancamentoId = lancamento.Id,
            Descricao    = cmd.Historico,
            Valor        = cmd.Valor,
            DataCriacao  = DateTime.UtcNow
        }, ct);

        return lancamento.Id;
    }
}
