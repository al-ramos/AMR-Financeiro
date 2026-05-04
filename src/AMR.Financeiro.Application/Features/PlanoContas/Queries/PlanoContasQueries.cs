using MediatR;
using AMR.Financeiro.Application.Features.PlanoContas.Dtos;

namespace AMR.Financeiro.Application.Features.PlanoContas.Queries;

public record GetPlanoContasQuery(int CdFilial) : IRequest<IEnumerable<PlanoContasDto>>;

public record GetPlanoContasArvoreQuery(int CdFilial) : IRequest<IEnumerable<PlanoContasArvoreDto>>;

public record GetPlanoContasByIdQuery(int Id) : IRequest<PlanoContasDto?>;
