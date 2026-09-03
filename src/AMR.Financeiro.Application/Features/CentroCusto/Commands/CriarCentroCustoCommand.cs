using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Commands;

// Observação: dentro deste namespace o identificador "CentroCusto" resolve para o
// próprio namespace Features.CentroCusto — por isso a entidade é referenciada
// de forma qualificada (Domain.Entities.CentroCusto), como em Features/PlanoDeContas.
public record CriarCentroCustoCommand(int CdFilial, string Codigo, string Descricao,
    TipoCentroCusto Tipo, int? PaiId, int Nivel, string ResponsavelNome) : IRequest<int>;

public class CriarCentroCustoHandler(ICentroCustoRepository repo)
    : IRequestHandler<CriarCentroCustoCommand, int>
{
    public async Task<int> Handle(CriarCentroCustoCommand cmd, CancellationToken ct)
    {
        // Pai, se informado, deve existir
        if (cmd.PaiId.HasValue)
        {
            var pai = await repo.GetByIdAsync(cmd.PaiId.Value, ct);
            if (pai is null)
                throw new InvalidOperationException($"Centro de custo pai com Id {cmd.PaiId} não encontrado.");
        }

        var cc = new Domain.Entities.CentroCusto(
            cmd.CdFilial, cmd.Codigo, cmd.Descricao, cmd.Tipo,
            cmd.PaiId, cmd.Nivel, cmd.ResponsavelNome);

        await repo.AddAsync(cc, ct);
        return cc.Id;
    }
}
