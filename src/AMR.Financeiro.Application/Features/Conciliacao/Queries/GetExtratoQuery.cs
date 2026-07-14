using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Conciliacao.Queries;

public record GetExtratoQuery(int ExtratoId) : IRequest<ExtratoDetalheDto?>;

public record ExtratoDetalheDto(
    int Id, string Banco, string ContaCorrente,
    DateOnly DataInicio, DateOnly DataFim,
    decimal SaldoInicial, decimal SaldoFinal,
    int TotalMovimentacoes, int Conciliados, int Pendentes, int Ignorados,
    List<MovimentacaoDto> Movimentacoes);

public record MovimentacaoDto(
    int Id, DateOnly DataLancamento, TipoMovimentacao Tipo,
    decimal Valor, string Descricao, string? CodigoDoc,
    StatusConciliacao Status, int? LancamentoId, string? ConciliadoPor, DateTime? ConciliadoEm);

public class GetExtratoQueryHandler(IConciliacaoRepository repo)
    : IRequestHandler<GetExtratoQuery, ExtratoDetalheDto?>
{
    public async Task<ExtratoDetalheDto?> Handle(GetExtratoQuery query, CancellationToken ct)
    {
        var extrato = await repo.GetExtratoByIdAsync(query.ExtratoId, ct);
        if (extrato is null) return null;

        var movimentacoes = await repo.GetMovimentacoesByExtratoAsync(query.ExtratoId, ct);
        var dtos = movimentacoes
            .Select(m => new MovimentacaoDto(
                m.Id, m.DataLancamento, m.Tipo, m.Valor, m.Descricao, m.CodigoDoc,
                m.StatusConciliacao, m.LancamentoId, m.ConciliadoPor, m.ConciliadoEm))
            .ToList();

        return new ExtratoDetalheDto(
            extrato.Id, extrato.Banco, extrato.ContaCorrente,
            extrato.DataInicio, extrato.DataFim,
            extrato.SaldoInicial, extrato.SaldoFinal,
            dtos.Count,
            dtos.Count(d => d.Status == StatusConciliacao.Conciliado),
            dtos.Count(d => d.Status == StatusConciliacao.Pendente),
            dtos.Count(d => d.Status == StatusConciliacao.Ignorado),
            dtos);
    }
}
