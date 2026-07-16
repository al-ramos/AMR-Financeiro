using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

public interface IContaBancariaRepository
{
    Task<ContaBancaria?> ObterPorIdAsync(int id, CancellationToken ct = default);

    Task<List<ContaBancaria>> ListarAsync(bool incluirInativas, CancellationToken ct = default);

    /// <summary>
    /// Saldo atual por conta, calculado em uma única query:
    /// SaldoInicial + créditos − débitos dos lançamentos vinculados desde DataSaldoInicial.
    /// </summary>
    Task<Dictionary<int, decimal>> ObterSaldosAsync(bool incluirInativas, CancellationToken ct = default);

    /// <summary>Saldo atual de uma única conta (mesma regra de ObterSaldosAsync).</summary>
    Task<decimal?> ObterSaldoAsync(int contaId, CancellationToken ct = default);

    /// <summary>Lançamentos vinculados à conta, mais recentes primeiro (extrato).</summary>
    Task<List<LancamentoFinanceiro>> ObterExtratoAsync(int contaId, CancellationToken ct = default);

    Task AdicionarAsync(ContaBancaria conta, CancellationToken ct = default);
    Task AtualizarAsync(ContaBancaria conta, CancellationToken ct = default);
}
