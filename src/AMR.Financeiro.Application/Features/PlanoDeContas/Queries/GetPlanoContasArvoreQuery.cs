using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Queries;

/// <summary>Árvore hierárquica do plano de contas (usada pela página Plano de Contas do frontend).</summary>
public record GetPlanoContasArvoreQuery(int CdFilial, bool IncluirInativos = false)
    : IRequest<List<ContaArvoreDto>>;

public record ContaArvoreDto(
    int Id, int CdFilial, string Codigo, string Descricao, TipoContaContabil Tipo,
    NaturezaConta Natureza, int Nivel, int? PaiId, GrupoDRE GrupoDRE,
    int OrdemExibicao, bool AceitaLancamentos, bool Ativo,
    List<ContaArvoreDto> Filhos);

public class GetPlanoContasArvoreHandler(IPlanoDeContasRepository repo)
    : IRequestHandler<GetPlanoContasArvoreQuery, List<ContaArvoreDto>>
{
    public async Task<List<ContaArvoreDto>> Handle(GetPlanoContasArvoreQuery q, CancellationToken ct)
    {
        var todas = await repo.GetByCdFilialAsync(q.CdFilial, q.IncluirInativos, ct);
        return BuildArvore(todas, null);
    }

    private static List<ContaArvoreDto> BuildArvore(
        List<Domain.Entities.PlanoDeContas> todas, int? paiId)
    {
        return todas
            .Where(c => c.PaiId == paiId)
            .OrderBy(c => c.OrdemExibicao)
            .ThenBy(c => c.Codigo)
            .Select(c => new ContaArvoreDto(
                c.Id, c.CdFilial, c.Codigo, c.Descricao, c.Tipo, c.Natureza,
                c.Nivel, c.PaiId, c.GrupoDRE, c.OrdemExibicao,
                c.AceitaLancamentos, c.Ativo,
                BuildArvore(todas, c.Id)))
            .ToList();
    }
}
