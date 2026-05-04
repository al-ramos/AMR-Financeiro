using MediatR;
using AMR.Financeiro.Application.Features.Lancamentos.Dtos;
using AMR.Financeiro.Application.Features.Lancamentos.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Lancamentos.Handlers;

public class GetLancamentosHandler(ILancamentoFinanceiroRepository repo)
    : IRequestHandler<GetLancamentosQuery, IEnumerable<LancamentoFinanceiroDto>>
{
    public async Task<IEnumerable<LancamentoFinanceiroDto>> Handle(GetLancamentosQuery q, CancellationToken ct)
    {
        var lista = await repo.ObterPorFilialAsync(q.CdFilial, ct);
        return lista.Select(ToDto);
    }

    internal static LancamentoFinanceiroDto ToDto(LancamentoFinanceiro l) => new(
        l.Id, l.CdFilial,
        l.PlanoContasId, l.PlanoContas?.Codigo ?? "", l.PlanoContas?.Descricao ?? "",
        l.Tipo.ToString(), l.Origem.ToString(),
        l.Valor, l.DataLancamento, l.Historico,
        l.DocumentoOrigemId, l.CriadoEm);
}

public class GetLancamentosByPeriodoHandler(ILancamentoFinanceiroRepository repo)
    : IRequestHandler<GetLancamentosByPeriodoQuery, IEnumerable<LancamentoFinanceiroDto>>
{
    public async Task<IEnumerable<LancamentoFinanceiroDto>> Handle(GetLancamentosByPeriodoQuery q, CancellationToken ct)
    {
        var lista = await repo.ObterPorPeriodoAsync(q.CdFilial, q.Inicio, q.Fim, ct);
        return lista.Select(GetLancamentosHandler.ToDto);
    }
}

public class GetLancamentosByPlanoContasHandler(ILancamentoFinanceiroRepository repo)
    : IRequestHandler<GetLancamentosByPlanoContasQuery, IEnumerable<LancamentoFinanceiroDto>>
{
    public async Task<IEnumerable<LancamentoFinanceiroDto>> Handle(GetLancamentosByPlanoContasQuery q, CancellationToken ct)
    {
        var lista = await repo.ObterPorPlanoContasAsync(q.PlanoContasId, ct);
        return lista.Select(GetLancamentosHandler.ToDto);
    }
}

public class GetLancamentoByIdHandler(ILancamentoFinanceiroRepository repo)
    : IRequestHandler<GetLancamentoByIdQuery, LancamentoFinanceiroDto?>
{
    public async Task<LancamentoFinanceiroDto?> Handle(GetLancamentoByIdQuery q, CancellationToken ct)
    {
        var l = await repo.ObterPorIdAsync(q.Id, ct);
        return l is null ? null : GetLancamentosHandler.ToDto(l);
    }
}
