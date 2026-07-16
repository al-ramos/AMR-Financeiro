using MediatR;
using AMR.Financeiro.Application.Features.ContasBancarias.Dtos;

namespace AMR.Financeiro.Application.Features.ContasBancarias.Queries;

public record GetContasBancariasQuery(bool IncluirInativas = false)
    : IRequest<List<ContaBancariaDto>>;

public record GetContaBancariaByIdQuery(int Id)
    : IRequest<ContaBancariaDto?>;

public record GetExtratoQuery(int ContaId)
    : IRequest<List<ExtratoItemDto>>;
