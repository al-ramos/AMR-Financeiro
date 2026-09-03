using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Application.Interfaces;

public record MatchSugestao(
    int LancamentoId,
    string LancamentoDescricao,
    decimal LancamentoValor,
    DateOnly LancamentoData,
    int Score,          // 0-100
    bool AutoConciliar); // true se score >= 70

public interface IConciliacaoMatchingService
{
    Task<List<MatchSugestao>> BuscarSugestoesAsync(
        MovimentacaoBancaria movimentacao,
        CancellationToken ct = default);
}
