using MediatR;
using RDS.Financeiro.Application.Features.PlanoContas.Dtos;

namespace RDS.Financeiro.Application.Features.PlanoContas.Queries;

public record GetPlanoContasQuery(int CdFilial) : IRequest<IEnumerable<PlanoContasDto>>;

public record GetPlanoContasArvoreQuery(int CdFilial) : IRequest<IEnumerable<PlanoContasArvoreDto>>;

public record GetPlanoContasByIdQuery(int Id) : IRequest<PlanoContasDto?>;
