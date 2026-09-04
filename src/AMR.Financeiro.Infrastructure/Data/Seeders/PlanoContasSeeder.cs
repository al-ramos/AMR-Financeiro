using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Data.Seeders;

/// <summary>
/// Plano de contas padrão do módulo — patrimoniais (1 e 2) e de resultado (3 a 6).
/// As folhas de nível 3 são analíticas e aceitam lançamento; as demais agrupam.
///
/// As patrimoniais existem porque as baixas de ContasPagar e ContasReceber liquidam
/// passivo e ativo (2.1.2 e 1.1.3) — sem elas a baixa fica sem contrapartida.
///
/// Roda no startup e também sob demanda via POST /api/plano-contas/seed; é idempotente
/// por (CdFilial, Codigo).
/// A persistência é feita nível a nível porque o PaiId depende do Id gerado pelo banco
/// para a conta pai (identity) — por isso o método é assíncrono e recebe o DbContext,
/// em vez da assinatura síncrona pura sugerida no card.
/// </summary>
public static class PlanoContasSeeder
{
    private sealed record Def(
        string Codigo,
        string? CodigoPai,
        string Descricao,
        TipoContaContabil Tipo,
        NaturezaConta Natureza,
        int Nivel,
        GrupoDRE Grupo,
        bool Analitica = false);

    private static readonly Def[] ContasPadrao =
    [
        // ── Patrimoniais ────────────────────────────────────────────────────────
        // Não compõem a DRE (GrupoDRE.NaoAplicavel), mas são o destino das baixas:
        // PagarContaHandler lança em 2.1.2 e ReceberContaHandler em 1.1.3. Sem elas
        // no plano, a baixa não tem contrapartida.
        new("1", null, "ATIVO", TipoContaContabil.Ativo, NaturezaConta.Devedora, 1, GrupoDRE.NaoAplicavel),
        new("2", null, "PASSIVO", TipoContaContabil.Passivo, NaturezaConta.Credora, 1, GrupoDRE.NaoAplicavel),

        new("1.1", "1", "Ativo Circulante", TipoContaContabil.Ativo, NaturezaConta.Devedora, 2, GrupoDRE.NaoAplicavel),
        new("2.1", "2", "Passivo Circulante", TipoContaContabil.Passivo, NaturezaConta.Credora, 2, GrupoDRE.NaoAplicavel),

        new("1.1.1", "1.1", "Caixa", TipoContaContabil.Ativo, NaturezaConta.Devedora, 3, GrupoDRE.NaoAplicavel, Analitica: true),
        new("1.1.2", "1.1", "Bancos", TipoContaContabil.Ativo, NaturezaConta.Devedora, 3, GrupoDRE.NaoAplicavel, Analitica: true),
        new("1.1.3", "1.1", "Contas a Receber", TipoContaContabil.Ativo, NaturezaConta.Devedora, 3, GrupoDRE.NaoAplicavel, Analitica: true),
        new("2.1.1", "2.1", "Fornecedores", TipoContaContabil.Passivo, NaturezaConta.Credora, 3, GrupoDRE.NaoAplicavel, Analitica: true),
        new("2.1.2", "2.1", "Contas a Pagar", TipoContaContabil.Passivo, NaturezaConta.Credora, 3, GrupoDRE.NaoAplicavel, Analitica: true),

        // ── Nível 1 — Grupos de resultado ───────────────────────────────────────
        // GrupoDRE em conta sintética é apenas classificatório: a DRE soma as analíticas.
        new("3", null, "RECEITAS", TipoContaContabil.Receita, NaturezaConta.Credora, 1, GrupoDRE.ReceitaBruta),
        new("4", null, "CUSTOS", TipoContaContabil.Custo, NaturezaConta.Devedora, 1, GrupoDRE.CustoMercadorias),
        new("5", null, "DESPESAS OPERACIONAIS", TipoContaContabil.Despesa, NaturezaConta.Devedora, 1, GrupoDRE.DespesasOperacionais),
        new("6", null, "RESULTADO FINANCEIRO", TipoContaContabil.OutrasDespesas, NaturezaConta.Devedora, 1, GrupoDRE.ResultadoFinanceiro),

        // ── Nível 2 — Subgrupos ─────────────────────────────────────────────────
        new("3.1", "3", "Receita Bruta", TipoContaContabil.Receita, NaturezaConta.Credora, 2, GrupoDRE.ReceitaBruta),
        new("3.2", "3", "Deduções", TipoContaContabil.Imposto, NaturezaConta.Devedora, 2, GrupoDRE.DeducoesReceita),
        new("4.1", "4", "CPV/CSP", TipoContaContabil.Custo, NaturezaConta.Devedora, 2, GrupoDRE.CustoMercadorias),
        new("5.1", "5", "Despesas Comerciais", TipoContaContabil.Despesa, NaturezaConta.Devedora, 2, GrupoDRE.DespesasOperacionais),
        new("5.2", "5", "Despesas Administrativas", TipoContaContabil.Despesa, NaturezaConta.Devedora, 2, GrupoDRE.DespesasOperacionais),
        new("6.1", "6", "Receitas Financeiras", TipoContaContabil.OutrasReceitas, NaturezaConta.Credora, 2, GrupoDRE.ReceitasFinanceiras),
        new("6.2", "6", "Despesas Financeiras", TipoContaContabil.OutrasDespesas, NaturezaConta.Devedora, 2, GrupoDRE.DespesasFinanceiras),
        new("6.3", "6", "Impostos IR/CSLL", TipoContaContabil.Imposto, NaturezaConta.Devedora, 2, GrupoDRE.ImpostosRenda),

        // ── Nível 3 — Contas sintéticas ─────────────────────────────────────────
        new("3.1.1", "3.1", "Receita de Produtos", TipoContaContabil.Receita, NaturezaConta.Credora, 3, GrupoDRE.ReceitaBruta, Analitica: true),
        new("3.1.2", "3.1", "Receita de Serviços", TipoContaContabil.Receita, NaturezaConta.Credora, 3, GrupoDRE.ReceitaBruta, Analitica: true),
        new("3.2.1", "3.2", "ICMS sobre Vendas", TipoContaContabil.Imposto, NaturezaConta.Devedora, 3, GrupoDRE.DeducoesReceita, Analitica: true),
        new("3.2.2", "3.2", "PIS/COFINS", TipoContaContabil.Imposto, NaturezaConta.Devedora, 3, GrupoDRE.DeducoesReceita, Analitica: true),
        new("3.2.3", "3.2", "Devoluções e Abatimentos", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DeducoesReceita, Analitica: true),
        new("4.1.1", "4.1", "Custo dos Produtos Vendidos", TipoContaContabil.Custo, NaturezaConta.Devedora, 3, GrupoDRE.CustoMercadorias, Analitica: true),
        new("4.1.2", "4.1", "Custo dos Serviços Prestados", TipoContaContabil.Custo, NaturezaConta.Devedora, 3, GrupoDRE.CustoMercadorias, Analitica: true),
        new("5.1.1", "5.1", "Salários Comerciais", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DespesasOperacionais, Analitica: true),
        new("5.1.2", "5.1", "Comissões", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DespesasOperacionais, Analitica: true),
        new("5.2.1", "5.2", "Salários Administrativos", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DespesasOperacionais, Analitica: true),
        new("5.2.2", "5.2", "Aluguel", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DespesasOperacionais, Analitica: true),
        new("5.2.3", "5.2", "Energia e Utilities", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DespesasOperacionais, Analitica: true),
        new("5.2.4", "5.2", "Depreciação", TipoContaContabil.Despesa, NaturezaConta.Devedora, 3, GrupoDRE.DespesasOperacionais, Analitica: true),
        new("6.1.1", "6.1", "Juros Recebidos", TipoContaContabil.OutrasReceitas, NaturezaConta.Credora, 3, GrupoDRE.ReceitasFinanceiras, Analitica: true),
        new("6.1.2", "6.1", "Rendimentos de Aplicações", TipoContaContabil.OutrasReceitas, NaturezaConta.Credora, 3, GrupoDRE.ReceitasFinanceiras, Analitica: true),
        new("6.2.1", "6.2", "Juros Pagos", TipoContaContabil.OutrasDespesas, NaturezaConta.Devedora, 3, GrupoDRE.DespesasFinanceiras, Analitica: true),
        new("6.2.2", "6.2", "IOF e Tarifas Bancárias", TipoContaContabil.OutrasDespesas, NaturezaConta.Devedora, 3, GrupoDRE.DespesasFinanceiras, Analitica: true),
        new("6.3.1", "6.3", "IRPJ", TipoContaContabil.Imposto, NaturezaConta.Devedora, 3, GrupoDRE.ImpostosRenda, Analitica: true),
        new("6.3.2", "6.3", "CSLL", TipoContaContabil.Imposto, NaturezaConta.Devedora, 3, GrupoDRE.ImpostosRenda, Analitica: true),
    ];

    /// <summary>
    /// Aplica o plano padrão para a filial, ignorando códigos que já existem (idempotente).
    /// Persiste nível a nível para que os Ids dos pais estejam disponíveis para os filhos.
    /// </summary>
    /// <returns>Quantidade de contas criadas.</returns>
    public static async Task<int> SeedAsync(FinanceiroDbContext ctx, int cdFilial, CancellationToken ct = default)
    {
        var criadas = 0;
        var ordem = 1;
        var idPorCodigo = await ctx.PlanoDeContas
            .Where(c => c.CdFilial == cdFilial)
            .ToDictionaryAsync(c => c.Codigo, c => c.Id, ct);

        foreach (var nivel in new[] { 1, 2, 3 })
        {
            foreach (var def in ContasPadrao.Where(d => d.Nivel == nivel))
            {
                var ordemAtual = ordem++;
                if (idPorCodigo.ContainsKey(def.Codigo))
                    continue; // já existe — seed idempotente

                int? paiId = def.CodigoPai is null ? null : idPorCodigo[def.CodigoPai];

                var conta = new PlanoDeContas(
                    cdFilial, def.Codigo, def.Descricao, def.Tipo, def.Natureza,
                    def.Nivel, paiId, def.Grupo, ordemAtual, def.Analitica);

                ctx.PlanoDeContas.Add(conta);
                criadas++;
            }

            // Salva o nível inteiro antes de criar os filhos (gera os Ids dos pais)
            await ctx.SaveChangesAsync(ct);

            foreach (var conta in ctx.PlanoDeContas.Local.Where(c => c.CdFilial == cdFilial))
                idPorCodigo[conta.Codigo] = conta.Id;
        }

        return criadas;
    }
}
