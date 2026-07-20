using MediatR;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Queries;

public record GetContaByIdQuery(int Id) : IRequest<ContaContabilDto?>;

public class GetContaByIdHandler(IPlanoDeContasRepository repo)
    : IRequestHandler<GetContaByIdQuery, ContaContabilDto?>
{
    public async Task<ContaContabilDto?> Handle(GetContaByIdQuery q, CancellationToken ct)
    {
        var c = await repo.GetByIdAsync(q.Id, ct);
        return c is null
            ? null
            : new ContaContabilDto(
                c.Id, c.Codigo, c.Descricao, c.Tipo, c.Natureza, c.Nivel,
                c.PaiId, c.GrupoDRE, c.OrdemExibicao, c.AceitaLancamentos, c.Ativo);
    }
}
