using RDS.Financeiro.Domain.Entities;

namespace RDS.Financeiro.Domain.Interfaces;

public interface ILancamentoFinanceiroRepository
{
    Task<LancamentoFinanceiro?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<LancamentoFinanceiro>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default);
    Task<IEnumerable<LancamentoFinanceiro>> ObterPorPlanoContasAsync(int planoContasId, CancellationToken ct = default);
    Task<IEnumerable<LancamentoFinanceiro>> ObterPorPeriodoAsync(int cdFilial, DateOnly inicio, DateOnly fim, CancellationToken ct = default);
    Task AdicionarAsync(LancamentoFinanceiro lancamento, CancellationToken ct = default);
}
