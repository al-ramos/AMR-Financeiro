namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Registro de um rateio executado para uma competência (Card 23.5).
/// </summary>
public class RateioRealizado
{
    public int Id { get; private set; }
    public int RegraRateioId { get; private set; }
    public int CentroCustoId { get; private set; }
    public decimal ValorRateado { get; private set; }
    public decimal PercentualAplicado { get; private set; }
    public DateOnly Competencia { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected RateioRealizado() { }

    public RateioRealizado(int regraRateioId, int centroCustoId, decimal valorRateado,
        decimal percentualAplicado, DateOnly competencia)
    {
        RegraRateioId = regraRateioId;
        CentroCustoId = centroCustoId;
        ValorRateado = valorRateado;
        PercentualAplicado = percentualAplicado;
        Competencia = competencia;
    }
}
