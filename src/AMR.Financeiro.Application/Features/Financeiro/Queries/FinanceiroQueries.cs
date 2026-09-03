using MediatR;
using AMR.Financeiro.Application.Features.Financeiro.Dtos;

namespace AMR.Financeiro.Application.Features.Financeiro.Queries;

public record GetAgingQuery : IRequest<AgingDto>;

public record GetFluxoCaixaQuery(int HorizonteDias = 30) : IRequest<FluxoCaixaDto>;
