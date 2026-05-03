using MediatR;
using RDS.Financeiro.Application.Features.ContasPagar.Dtos;

namespace RDS.Financeiro.Application.Features.ContasPagar.Queries;

public record GetContasPagarQuery(int CdFilial) : IRequest<IEnumerable<ContaPagarDto>>;
public record GetContaPagarByIdQuery(int Id) : IRequest<ContaPagarDto?>;
