using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Interfaces;

/// <summary>Linha da DRE com comparativo de períodos e detalhamento por conta analítica.</summary>
public record LinhasDRE(
    GrupoDRE Grupo,
    string Descricao,
    decimal ValorAtual,
    decimal ValorPeriodoAnterior,
    decimal ValorMesmoMesAnoAnterior,
    decimal VariacaoMes,       // % vs periodo anterior
    decimal VariacaoAno,       // % vs mesmo mes ano passado
    bool Negrito,
    bool EhSubtotal,
    List<ContaDRE> Contas);

public record ContaDRE(string Codigo, string Descricao, decimal Valor);

public record DreResult(
    string Periodo,
    List<LinhasDRE> Linhas,
    decimal MargemBruta,       // LucroBruto / ReceitaLiquida * 100
    decimal MargemOperacional, // ResultadoOperacional / ReceitaLiquida * 100
    decimal MargemLiquida);    // LucroLiquido / ReceitaLiquida * 100

public interface IDreService
{
    Task<DreResult> CalcularAsync(int cdFilial, int ano, int mes, CancellationToken ct = default);
}
