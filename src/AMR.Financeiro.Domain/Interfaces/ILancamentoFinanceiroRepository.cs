using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

public interface ILancamentoFinanceiroRepository
{
    Task<LancamentoFinanceiro?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<LancamentoFinanceiro>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default);
    Task<IEnumerable<LancamentoFinanceiro>> ObterPorPlanoContasAsync(int planoContasId, CancellationToken ct = default);
    Task<IEnumerable<LancamentoFinanceiro>> ObterPorPeriodoAsync(int cdFilial, DateOnly inicio, DateOnly fim, CancellationToken ct = default);
    /// <summary>Lançamentos com data futura dentro da janela — entradas/saídas previstas do fluxo de caixa (Card 23.6).</summary>
    Task<List<LancamentoFinanceiro>> ObterFuturosAsync(DateOnly aPartirDe, DateOnly ate, CancellationToken ct = default);
    Task AdicionarAsync(LancamentoFinanceiro lancamento, CancellationToken ct = default);
    Task AtualizarAsync(LancamentoFinanceiro lancamento, CancellationToken ct = default);
}
