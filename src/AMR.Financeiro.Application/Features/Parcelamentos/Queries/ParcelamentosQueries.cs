using MediatR;
using AMR.Financeiro.Application.Features.Parcelamentos.Dtos;

namespace AMR.Financeiro.Application.Features.Parcelamentos.Queries;

public record GetParcelamentosQuery : IRequest<List<ParcelamentoDto>>;

public record GetParcelamentoByIdQuery(int Id) : IRequest<ParcelamentoDto?>;
