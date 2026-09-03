namespace AMR.Financeiro.Domain.Enums;

public enum TipoBaseRateio
{
    /// <summary>Percentuais fixos definidos por destino.</summary>
    FixoPercentual,

    /// <summary>Percentual calculado proporcionalmente à área (m²) de cada destino.</summary>
    AreaM2,

    /// <summary>Percentual calculado proporcionalmente ao headcount de cada destino.</summary>
    Headcount
}
