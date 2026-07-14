using MediatR;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Commands;

public record AtualizarOrcamentoCommand(int CentroCustoId, string ContaDescricao, int Ano, int Mes, decimal ValorOrcado) : IRequest<bool>;

public class AtualizarOrcamentoHandler(ICentroCustoRepository repo)
    : IRequestHandler<AtualizarOrcamentoCommand, bool>
{
    public async Task<bool> Handle(AtualizarOrcamentoCommand cmd, CancellationToken ct)
    {
        var cc = await repo.GetByIdAsync(cmd.CentroCustoId, ct);
        if (cc is null)
            throw new InvalidOperationException($"Centro de custo com Id {cmd.CentroCustoId} não encontrado.");

        // O repositório busca por (CentroCustoId, ContaDescricao, Ano, Mes):
        // se já existe, atualiza o ValorOrcado preservando o realizado; senão, insere.
        var orcamento = new OrcamentoCC(cmd.CentroCustoId, cmd.ContaDescricao, cmd.Ano, cmd.Mes, cmd.ValorOrcado);
        await repo.UpsertOrcamentoAsync(orcamento, ct);
        return true;
    }
}
