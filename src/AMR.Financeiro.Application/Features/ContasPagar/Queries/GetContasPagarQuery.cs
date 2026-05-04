using MediatR;
using AMR.Financeiro.Application.Features.ContasPagar.Dtos;

namespace AMR.Financeiro.Application.Features.ContasPagar.Queries;

public record GetContasPagarQuery(int CdFilial) : IRequest<IEnumerable<ContaPagarDto>>;
public record GetContaPagarByIdQuery(int Id) : IRequest<ContaPagarDto?>;
