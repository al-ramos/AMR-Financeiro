using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Calcula a DRE (Demonstração de Resultado) por competência mensal, com comparativo
/// contra o período anterior e o mesmo mês do ano anterior, e margens sobre a receita líquida.
///
/// Fonte dos valores: a tabela Lancamentos (LancamentoFinanceiro), que é o razão do sistema —
/// ela já consolida os recebimentos de ContasReceber (Tipo=Credito, Origem=ContaReceber),
/// os pagamentos de ContasPagar (Tipo=Debito, Origem=ContaPagar) e lançamentos manuais.
/// LancamentoFinanceiro.PlanoContasId referencia diretamente a conta em planodecontas —
/// o plano é único desde o FIN-01, e não há mais ponte por (CdFilial, Codigo).
/// </summary>
public class DreService(FinanceiroDbContext ctx) : IDreService
{
    private static readonly Dictionary<GrupoDRE, bool> EhSubtotal = new()
    {
        [GrupoDRE.ReceitaLiquida] = true,
        [GrupoDRE.LucroBruto] = true,
        [GrupoDRE.ResultadoOperacional] = true,
        [GrupoDRE.ResultadoFinanceiro] = true,
        [GrupoDRE.ResultadoAntesIR] = true,
        [GrupoDRE.LucroLiquido] = true,
    };

    public async Task<DreResult> CalcularAsync(int cdFilial, int ano, int mes, CancellationToken ct = default)
    {
        // Contas analíticas de resultado. As patrimoniais (GrupoDRE.NaoAplicavel)
        // existem no razão, recebem as baixas de CP/CR, e não entram em linha nenhuma.
        var contas = await ctx.PlanoDeContas
            .AsNoTracking()
            .Where(c => c.CdFilial == cdFilial && c.Ativo
                     && c.AceitaLancamentos
                     && c.GrupoDRE != GrupoDRE.NaoAplicavel)
            .OrderBy(c => c.OrdemExibicao)
            .ThenBy(c => c.Codigo)
            .ToListAsync(ct);

        // Períodos: atual, anterior (mes-1; se mes=1 → 12/ano-1) e mesmo mês do ano anterior
        var (anoAnt, mesAnt) = mes == 1 ? (ano - 1, 12) : (ano, mes - 1);

        var saldosAtual = await CalcularSaldosPorContaAsync(cdFilial, contas, ano, mes, ct);
        var saldosMesAnterior = await CalcularSaldosPorContaAsync(cdFilial, contas, anoAnt, mesAnt, ct);
        var saldosAnoAnterior = await CalcularSaldosPorContaAsync(cdFilial, contas, ano - 1, mes, ct);

        // Totais por grupo em cada período — subtotais preenchidos progressivamente
        var valoresAtual = TotaisPorGrupo(contas, saldosAtual);
        var valoresMesAnterior = TotaisPorGrupo(contas, saldosMesAnterior);
        var valoresAnoAnterior = TotaisPorGrupo(contas, saldosAnoAnterior);

        var linhas = new List<LinhasDRE>();

        foreach (var grupo in Enum.GetValues<GrupoDRE>())
        {
            if (grupo == GrupoDRE.NaoAplicavel) continue;

            var subtotal = EhSubtotal.GetValueOrDefault(grupo);

            if (subtotal)
            {
                // Subtotais são calculados por fórmula, não por contas
                valoresAtual[grupo] = CalcularSubtotal(grupo, valoresAtual);
                valoresMesAnterior[grupo] = CalcularSubtotal(grupo, valoresMesAnterior);
                valoresAnoAnterior[grupo] = CalcularSubtotal(grupo, valoresAnoAnterior);
            }

            var valorAtual = valoresAtual[grupo];
            var valorMesAnt = valoresMesAnterior[grupo];
            var valorAnoAnt = valoresAnoAnterior[grupo];

            var contasDoGrupo = subtotal
                ? new List<ContaDRE>()
                : contas
                    .Where(c => c.GrupoDRE == grupo)
                    .Select(c => new ContaDRE(c.Codigo, c.Descricao, saldosAtual.GetValueOrDefault(c.Id)))
                    .ToList();

            linhas.Add(new LinhasDRE(
                Grupo: grupo,
                Descricao: DescricaoDoGrupo(grupo),
                ValorAtual: valorAtual,
                ValorPeriodoAnterior: valorMesAnt,
                ValorMesmoMesAnoAnterior: valorAnoAnt,
                VariacaoMes: CalcularVariacao(valorAtual, valorMesAnt),
                VariacaoAno: CalcularVariacao(valorAtual, valorAnoAnt),
                Negrito: subtotal,
                EhSubtotal: subtotal,
                Contas: contasDoGrupo));
        }

        // Margens sobre a receita líquida
        var receitaLiquida = valoresAtual[GrupoDRE.ReceitaLiquida];

        return new DreResult(
            Periodo: $"{mes:D2}/{ano}",
            Linhas: linhas,
            MargemBruta: Margem(valoresAtual[GrupoDRE.LucroBruto], receitaLiquida),
            MargemOperacional: Margem(valoresAtual[GrupoDRE.ResultadoOperacional], receitaLiquida),
            MargemLiquida: Margem(valoresAtual[GrupoDRE.LucroLiquido], receitaLiquida));
    }

    /// <summary>
    /// Saldo de cada conta analítica no período (chave = Id da conta no plano novo).
    /// Conta Credora: SUM(créditos) - SUM(débitos); Conta Devedora: SUM(débitos) - SUM(créditos).
    /// </summary>
    private async Task<Dictionary<int, decimal>> CalcularSaldosPorContaAsync(
        int cdFilial, List<PlanoDeContas> contas, int ano, int mes, CancellationToken ct)
    {
        var inicio = new DateOnly(ano, mes, 1);
        var fim = inicio.AddMonths(1).AddDays(-1);

        // Lançamentos do período agrupados pela conta e pelo tipo (Débito/Crédito).
        // Inclui recebimentos (ContasReceber), pagamentos (ContasPagar) e lançamentos manuais,
        // pois todos são materializados na tabela Lancamentos.
        //
        // A agregação é por PlanoContasId direto. Antes havia uma ponte por Codigo entre o
        // plano legado e este — e era ela que deixava o Id da tela ser resolvido contra a
        // outra tabela, contabilizando o lançamento em outra conta (FIN-01).
        var somas = await ctx.Lancamentos.AsNoTracking()
            .Where(l => l.CdFilial == cdFilial
                     && l.DataLancamento >= inicio
                     && l.DataLancamento <= fim)
            .GroupBy(l => new { l.PlanoContasId, l.Tipo })
            .Select(g => new { g.Key.PlanoContasId, g.Key.Tipo, Total = g.Sum(x => x.Valor) })
            .ToListAsync(ct);

        var porConta = somas
            .GroupBy(s => s.PlanoContasId)
            .ToDictionary(
                g => g.Key,
                g => (Creditos: g.Where(x => x.Tipo == TipoLancamento.Credito).Sum(x => x.Total),
                      Debitos: g.Where(x => x.Tipo == TipoLancamento.Debito).Sum(x => x.Total)));

        var saldos = new Dictionary<int, decimal>();
        foreach (var conta in contas)
        {
            var (creditos, debitos) = porConta.GetValueOrDefault(conta.Id);
            saldos[conta.Id] = conta.Natureza == NaturezaConta.Credora
                ? creditos - debitos
                : debitos - creditos;
        }

        return saldos;
    }

    /// <summary>Soma dos saldos das contas analíticas por grupo (subtotais entram como 0 e são calculados depois).</summary>
    private static Dictionary<GrupoDRE, decimal> TotaisPorGrupo(
        List<PlanoDeContas> contas, Dictionary<int, decimal> saldos)
    {
        var valores = Enum.GetValues<GrupoDRE>().ToDictionary(g => g, _ => 0m);

        foreach (var conta in contas)
            valores[conta.GrupoDRE] += saldos.GetValueOrDefault(conta.Id);

        return valores;
    }

    private static decimal CalcularSubtotal(GrupoDRE grupo, Dictionary<GrupoDRE, decimal> valores) => grupo switch
    {
        GrupoDRE.ReceitaLiquida => valores[GrupoDRE.ReceitaBruta] - valores[GrupoDRE.DeducoesReceita],
        GrupoDRE.LucroBruto => valores[GrupoDRE.ReceitaLiquida] - valores[GrupoDRE.CustoMercadorias],
        GrupoDRE.ResultadoOperacional => valores[GrupoDRE.LucroBruto] - valores[GrupoDRE.DespesasOperacionais],
        GrupoDRE.ResultadoFinanceiro => valores[GrupoDRE.ReceitasFinanceiras] - valores[GrupoDRE.DespesasFinanceiras],
        GrupoDRE.ResultadoAntesIR => valores[GrupoDRE.ResultadoOperacional] + valores[GrupoDRE.ResultadoFinanceiro],
        GrupoDRE.LucroLiquido => valores[GrupoDRE.ResultadoAntesIR] - valores[GrupoDRE.ImpostosRenda],
        _ => 0
    };

    private static decimal CalcularVariacao(decimal valorAtual, decimal valorAnterior) =>
        valorAtual != 0 && valorAnterior != 0
            ? Math.Round((valorAtual - valorAnterior) / Math.Abs(valorAnterior) * 100, 2)
            : 0;

    private static decimal Margem(decimal valor, decimal receitaLiquida) =>
        receitaLiquida != 0 ? Math.Round(valor / receitaLiquida * 100, 2) : 0;

    private static string DescricaoDoGrupo(GrupoDRE grupo) => grupo switch
    {
        GrupoDRE.ReceitaBruta => "Receita Bruta",
        GrupoDRE.DeducoesReceita => "(-) Deduções da Receita",
        GrupoDRE.ReceitaLiquida => "(=) Receita Líquida",
        GrupoDRE.CustoMercadorias => "(-) Custo das Mercadorias/Serviços",
        GrupoDRE.LucroBruto => "(=) Lucro Bruto",
        GrupoDRE.DespesasOperacionais => "(-) Despesas Operacionais",
        GrupoDRE.ResultadoOperacional => "(=) Resultado Operacional",
        GrupoDRE.ReceitasFinanceiras => "(+) Receitas Financeiras",
        GrupoDRE.DespesasFinanceiras => "(-) Despesas Financeiras",
        GrupoDRE.ResultadoFinanceiro => "(=) Resultado Financeiro",
        GrupoDRE.ResultadoAntesIR => "(=) Resultado Antes do IR/CSLL",
        GrupoDRE.ImpostosRenda => "(-) IRPJ e CSLL",
        GrupoDRE.LucroLiquido => "(=) Lucro Líquido do Período",
        _ => grupo.ToString()
    };
}
