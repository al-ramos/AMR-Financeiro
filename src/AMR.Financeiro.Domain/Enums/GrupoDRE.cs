namespace AMR.Financeiro.Domain.Enums;

/// <summary>
/// Grupos da Demonstração de Resultado do Exercício, na ordem de apresentação.
/// A ordem dos membros define a ordem das linhas do relatório.
/// </summary>
public enum GrupoDRE
{
    /// <summary>Conta patrimonial: existe no razao, mas nao entra em nenhuma linha da DRE.</summary>
    NaoAplicavel,

    ReceitaBruta,
    DeducoesReceita,
    ReceitaLiquida,
    CustoMercadorias,
    LucroBruto,
    DespesasOperacionais,
    ResultadoOperacional,
    ReceitasFinanceiras,
    DespesasFinanceiras,
    ResultadoFinanceiro,
    ResultadoAntesIR,
    ImpostosRenda,
    LucroLiquido
}
