namespace AMR.Financeiro.Application.Interfaces;

public record RateioExecucaoResult(int TotalRegras, int TotalRateios, decimal ValorTotalRateado, List<string> Erros);

/// <summary>
/// Executa o rateio automático de uma competência (mês) para uma filial (Card 23.5).
/// </summary>
public interface IRateioService
{
    /// <exception cref="InvalidOperationException">Quando o rateio da competência já foi executado.</exception>
    Task<RateioExecucaoResult> ExecutarMesAsync(int cdFilial, DateOnly competencia, CancellationToken ct = default);
}
