using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Queries;

public record GetPlanoContasQuery(int CdFilial, bool IncluirInativos = false) : IRequest<List<ContaContabilDto>>;

public record ContaContabilDto(
    int Id, string Codigo, string Descricao, TipoContaContabil Tipo,
    NaturezaConta Natureza, int Nivel, int? PaiId, GrupoDRE GrupoDRE,
    int OrdemExibicao, bool AceitaLancamentos, bool Ativo);

public class GetPlanoContasHandler(IPlanoDeContasRepository repo)
    : IRequestHandler<GetPlanoContasQuery, List<ContaContabilDto>>
{
    public async Task<List<ContaContabilDto>> Handle(GetPlanoContasQuery q, CancellationToken ct)
    {
        var contas = await repo.GetByCdFilialAsync(q.CdFilial, q.IncluirInativos, ct);
        return contas
            .Select(c => new ContaContabilDto(
                c.Id, c.Codigo, c.Descricao, c.Tipo, c.Natureza, c.Nivel,
                c.PaiId, c.GrupoDRE, c.OrdemExibicao, c.AceitaLancamentos, c.Ativo))
            .ToList();
    }
}
