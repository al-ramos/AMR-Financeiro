using MediatR;
using AMR.Financeiro.Application.Features.Lancamentos.Dtos;

namespace AMR.Financeiro.Application.Features.Lancamentos.Queries;

public record GetLancamentosQuery(int CdFilial) : IRequest<IEnumerable<LancamentoFinanceiroDto>>;

public record GetLancamentosByPeriodoQuery(
    int CdFilial,
    DateOnly Inicio,
    DateOnly Fim
) : IRequest<IEnumerable<LancamentoFinanceiroDto>>;

public record GetLancamentosByPlanoContasQuery(
    int PlanoContasId
) : IRequest<IEnumerable<LancamentoFinanceiroDto>>;

public record GetLancamentoByIdQuery(int Id) : IRequest<LancamentoFinanceiroDto?>;
