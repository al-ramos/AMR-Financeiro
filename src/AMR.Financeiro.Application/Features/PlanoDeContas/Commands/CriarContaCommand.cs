using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

// Módulo novo (Card 23.4) fica em Features/PlanoDeContas para não colidir com o
// módulo legado Features/PlanoContas (que já define GetPlanoContasQuery etc.).
public record CriarContaCommand(
    int CdFilial, string Codigo, string Descricao,
    TipoContaContabil Tipo, NaturezaConta Natureza,
    int Nivel, int? PaiId, GrupoDRE GrupoDre, int OrdemExibicao)
    : IRequest<int>; // retorna Id criado

public class CriarContaHandler(IPlanoDeContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<CriarContaCommand, int>
{
    public async Task<int> Handle(CriarContaCommand cmd, CancellationToken ct)
    {
        // Código deve ser único por filial
        var existente = await repo.GetByCodigoAsync(cmd.CdFilial, cmd.Codigo, ct);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe uma conta com o código '{cmd.Codigo}' para esta filial.");

        // Pai, se informado, deve existir
        if (cmd.PaiId.HasValue)
        {
            var pai = await repo.GetByIdAsync(cmd.PaiId.Value, ct);
            if (pai is null)
                throw new InvalidOperationException($"Conta pai com Id {cmd.PaiId} não encontrada.");
        }

        var conta = new Domain.Entities.PlanoDeContas(
            cmd.CdFilial, cmd.Codigo, cmd.Descricao, cmd.Tipo, cmd.Natureza,
            cmd.Nivel, cmd.PaiId, cmd.GrupoDre, cmd.OrdemExibicao);

        await repo.AddAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return conta.Id;
    }
}
