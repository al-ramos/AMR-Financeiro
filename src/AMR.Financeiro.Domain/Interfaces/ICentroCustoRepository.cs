using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

/// <summary>
/// Repositório de centros de custo, orçamentos e rateios (Card 23.5).
/// Observação: diferentemente dos demais repositórios (que dependem de IUnitOfWork
/// no handler), os métodos de escrita deste repositório persistem imediatamente —
/// o contrato é consumido também pelo RateioService, que não recebe IUnitOfWork.
/// </summary>
public interface ICentroCustoRepository
{
    Task<List<CentroCusto>> GetByCdFilialAsync(int cdFilial, CancellationToken ct = default);
    Task<CentroCusto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(CentroCusto cc, CancellationToken ct = default);
    Task UpdateAsync(CentroCusto cc, CancellationToken ct = default);
    Task<List<OrcamentoCC>> GetOrcamentoAsync(int centroCustoId, int ano, CancellationToken ct = default);
    Task UpsertOrcamentoAsync(OrcamentoCC orcamento, CancellationToken ct = default);
    Task<List<RegraRateio>> GetRegrasAtivasAsync(int cdFilial, CancellationToken ct = default);
    Task AddRegraAsync(RegraRateio regra, List<RegraRateioDestino> destinos, CancellationToken ct = default);
    Task<bool> RateioJaExecutadoAsync(int cdFilial, DateOnly competencia, CancellationToken ct = default);
    Task AddRateiosAsync(List<RateioRealizado> rateios, CancellationToken ct = default);
    Task<List<OrcamentoCC>> GetAlertasAsync(int cdFilial, CancellationToken ct = default);
    Task<List<LancamentoFinanceiro>> GetLancamentosPorCentroCustoAsync(int centroCustoId, DateOnly inicio, DateOnly fim, CancellationToken ct = default);
    Task<List<RateioRealizado>> GetRateiosPorCentroCustoAsync(int centroCustoId, DateOnly inicio, DateOnly fim, CancellationToken ct = default);
    Task<List<RegraRateio>> GetRegrasPorIdsAsync(List<int> ids, CancellationToken ct = default);

    /// <summary>
    /// Total apurado na conta de origem dentro da competência — a base do rateio.
    /// Segue a natureza da conta: devedora soma débitos menos créditos, credora o inverso.
    /// Devolve null quando a conta não existe.
    /// </summary>
    Task<decimal?> ObterTotalDaContaAsync(int cdFilial, int contaOrigemId, DateOnly competencia, CancellationToken ct = default);
}
