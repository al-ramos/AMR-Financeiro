namespace AMR.Financeiro.Domain.Enums;

/// <summary>
/// Grupos da Demonstração de Resultado do Exercício, na ordem de apresentação.
/// A ordem dos membros define a ordem das linhas do relatório.
/// </summary>
public enum GrupoDRE
{
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
