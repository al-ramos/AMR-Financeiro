namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Destino de uma regra de rateio: centro de custo que recebe parte do valor (Card 23.5).
/// </summary>
public class RegraRateioDestino
{
    public int Id { get; private set; }
    public int RegraRateioId { get; private set; }
    public int CentroCustoId { get; private set; }

    /// <summary>Percentual fixo aplicado quando a base é FixoPercentual.</summary>
    public decimal Percentual { get; private set; }

    /// <summary>Valor da base dinâmica (m² ou headcount) quando a base não é FixoPercentual.</summary>
    public decimal? ValorBase { get; private set; }

    protected RegraRateioDestino() { }

    public RegraRateioDestino(int regraRateioId, int centroCustoId, decimal percentual, decimal? valorBase)
    {
        RegraRateioId = regraRateioId;
        CentroCustoId = centroCustoId;
        Percentual = percentual;
        ValorBase = valorBase;
    }
}
