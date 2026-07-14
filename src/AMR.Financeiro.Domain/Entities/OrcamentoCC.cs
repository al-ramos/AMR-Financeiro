namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Orçamento mensal por centro de custo e conta (Card 23.5).
/// A conta é referenciada pela descrição (sem FK) — a integração com o
/// plano de contas gerencial está planejada para o Sprint 25.
/// </summary>
public class OrcamentoCC
{
    public int Id { get; private set; }
    public int CentroCustoId { get; private set; }

    /// <summary>Nome da conta orçada (sem FK com o plano de contas).</summary>
    public string ContaDescricao { get; private set; } = string.Empty;

    public int Ano { get; private set; }
    public int Mes { get; private set; }
    public decimal ValorOrcado { get; private set; }
    public decimal ValorRealizado { get; private set; }

    /// <summary>Percentual consumido do orçamento (0 quando não há valor orçado).</summary>
    public decimal PercentualConsumido =>
        ValorOrcado == 0 ? 0 : ValorRealizado / ValorOrcado * 100;

    /// <summary>Consumo maior ou igual a 90% do orçado.</summary>
    public bool EmAlerta => PercentualConsumido >= 90;

    /// <summary>Consumo acima de 100% do orçado.</summary>
    public bool Estourado => PercentualConsumido > 100;

    protected OrcamentoCC() { }

    public OrcamentoCC(int centroCustoId, string contaDescricao, int ano, int mes,
        decimal valorOrcado, decimal valorRealizado = 0)
    {
        if (mes is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(mes), mes, "Mês deve estar entre 1 e 12.");

        CentroCustoId = centroCustoId;
        ContaDescricao = contaDescricao;
        Ano = ano;
        Mes = mes;
        ValorOrcado = valorOrcado;
        ValorRealizado = valorRealizado;
    }

    public void AtualizarOrcado(decimal valorOrcado) => ValorOrcado = valorOrcado;

    public void AtualizarRealizado(decimal valorRealizado) => ValorRealizado = valorRealizado;
}
