using MediatR;
using AMR.Financeiro.Application.Features.PlanoContas.Dtos;
using AMR.Financeiro.Application.Features.PlanoContas.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoContas.Handlers;

public class GetPlanoContasHandler(IPlanoContasRepository repo)
    : IRequestHandler<GetPlanoContasQuery, IEnumerable<PlanoContasDto>>
{
    public async Task<IEnumerable<PlanoContasDto>> Handle(GetPlanoContasQuery q, CancellationToken ct)
    {
        var contas = await repo.ObterPorFilialAsync(q.CdFilial, ct);
        return contas.Select(ToDto);
    }

    internal static PlanoContasDto ToDto(Domain.Entities.PlanoContas c) => new(
        c.Id, c.CdFilial, c.Codigo, c.Descricao,
        c.Tipo.ToString(), c.PaiId, c.Pai?.Descricao,
        c.Nivel(), c.Ativo, c.AceitaLancamentos());
}

public class GetPlanoContasByIdHandler(IPlanoContasRepository repo)
    : IRequestHandler<GetPlanoContasByIdQuery, PlanoContasDto?>
{
    public async Task<PlanoContasDto?> Handle(GetPlanoContasByIdQuery q, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(q.Id, ct);
        return conta is null ? null : GetPlanoContasHandler.ToDto(conta);
    }
}

public class GetPlanoContasArvoreHandler(IPlanoContasRepository repo)
    : IRequestHandler<GetPlanoContasArvoreQuery, IEnumerable<PlanoContasArvoreDto>>
{
    public async Task<IEnumerable<PlanoContasArvoreDto>> Handle(GetPlanoContasArvoreQuery q, CancellationToken ct)
    {
        var todas = (await repo.ObterPorFilialAsync(q.CdFilial, ct)).ToList();
        return BuildArvore(todas, null);
    }

    private static List<PlanoContasArvoreDto> BuildArvore(
        List<Domain.Entities.PlanoContas> todas, int? paiId)
    {
        return todas
            .Where(c => c.PaiId == paiId)
            .OrderBy(c => c.Codigo)
            .Select(c => new PlanoContasArvoreDto(
                c.Id, c.CdFilial, c.Codigo, c.Descricao,
                c.Tipo.ToString(), c.PaiId, c.Nivel(), c.Ativo, c.AceitaLancamentos(),
                BuildArvore(todas, c.Id)))
            .ToList();
    }
}
