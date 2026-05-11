using MediatR;
using AMR.Financeiro.Application.Features.ContasReceber.Dtos;

namespace AMR.Financeiro.Application.Features.ContasReceber.Queries;

public record GetContasReceberQuery(int CdFilial) : IRequest<IEnumerable<ContaReceberDto>>;
public record GetContaReceberByIdQuery(int Id) : IRequest<ContaReceberDto?>;
