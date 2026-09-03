namespace AMR.Financeiro.Domain.Enums;

/// <summary>Classificação contábil da conta no plano de contas gerencial (DRE).</summary>
public enum TipoContaContabil
{
    // Patrimoniais — nao compoem a DRE, mas sao o destino das baixas de
    // ContasPagar/ContasReceber, que liquidam passivo e ativo.
    Ativo,
    Passivo,

    // De resultado
    Receita,
    Custo,
    Despesa,
    Imposto,
    OutrasReceitas,
    OutrasDespesas
}
