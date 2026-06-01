using Microsoft.EntityFrameworkCore;

namespace AMR.Financeiro.Infrastructure.Data;

/// <summary>
/// Seed de lançamentos demo para o AMR-Financeiro.
/// Gera movimentos dos últimos 3 meses para popular o Dashboard e os relatórios.
/// Idempotente — só insere se não houver lançamentos para a filial.
/// </summary>
public static class LancamentosDemoSeed
{
    public static async Task AplicarAsync(FinanceiroDbContext ctx, int cdFilial = 1)
    {
        if (await ctx.Lancamentos.AnyAsync(l => l.CdFilial == cdFilial))
            return;

        // Busca contas analíticas pelo código
        async Task<int> ContaId(string codigo) =>
            await ctx.PlanoContas
                .Where(c => c.CdFilial == cdFilial && c.Codigo == codigo)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

        var idReceita    = await ContaId("3.1.1"); // Receita de Vendas
        var idServicos   = await ContaId("3.1.2"); // Receita de Serviços
        var idEnergia    = await ContaId("4.1.1"); // Energia Elétrica
        var idAluguel    = await ContaId("4.1.2"); // Aluguel
        var idTelefone   = await ContaId("4.1.3"); // Telefone e Internet
        var idSalarios   = await ContaId("4.2.1"); // Salários e Encargos
        var idProlabore  = await ContaId("4.2.2"); // Pró-labore
        var idMaterial   = await ContaId("4.2.3"); // Material de Escritório
        var idCMV        = await ContaId("5.1.1"); // CMV

        if (idReceita == 0 || idAluguel == 0) return; // Plano de contas não seedado

        var now  = DateTime.UtcNow;
        var rows = new List<(string data, int conta, string tipo, decimal valor, string historico)>();

        // ── Gera 3 meses de movimentos ────────────────────────────────────────
        for (int mes = 2; mes >= 0; mes--)
        {
            var refDate = new DateTime(now.Year, now.Month, 1).AddMonths(-mes);
            var daysInMonth = DateTime.DaysInMonth(refDate.Year, refDate.Month);

            string D(int day) => new DateTime(refDate.Year, refDate.Month, Math.Min(day, daysInMonth))
                                     .ToString("yyyy-MM-dd");

            // Receitas mensais
            rows.Add((D(5),  idReceita,   "Credito", 18_500.00m, "Faturamento — lote produção A"));
            rows.Add((D(12), idReceita,   "Credito", 12_300.00m, "Faturamento — pedido PV-002"));
            rows.Add((D(18), idReceita,   "Credito",  9_850.00m, "Faturamento — pedido PV-003"));
            rows.Add((D(25), idReceita,   "Credito", 15_200.00m, "Faturamento — pedido PV-004"));
            rows.Add((D(20), idServicos,  "Credito",  3_500.00m, "Assistência técnica mensal"));

            // Despesas fixas
            rows.Add((D(5),  idAluguel,   "Debito",  4_200.00m, "Aluguel galpão industrial — mai/26"));
            rows.Add((D(10), idEnergia,   "Debito",  1_850.00m, "Conta de energia — ENEL mai/26"));
            rows.Add((D(8),  idTelefone,  "Debito",    380.00m, "Internet fibra + PABX"));
            rows.Add((D(5),  idSalarios,  "Debito",  9_800.00m, "Folha de pagamento mensal"));
            rows.Add((D(5),  idProlabore, "Debito",  3_500.00m, "Pró-labore sócios"));
            rows.Add((D(15), idCMV,       "Debito",  8_200.00m, "CMV — custo mercadorias vendidas"));

            // Despesas variáveis
            rows.Add((D(7),  idMaterial,  "Debito",    245.00m, "Material de escritório — papelaria"));
            rows.Add((D(22), idEnergia,   "Debito",    120.00m, "Ar-condicionado escritório — manutenção"));
        }

        // Insere via SQL direto (evita sentinel bug EF Core 9 + SQLite)
        var criadoEm = now.ToString("o");
        foreach (var (data, conta, tipo, valor, historico) in rows)
        {
            await ctx.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""Lancamentos""
                    (""CdFilial"", ""PlanoContasId"", ""Tipo"", ""Origem"",
                     ""Valor"", ""DataLancamento"", ""Historico"", ""CriadoEm"")
                VALUES
                    ({cdFilial}, {conta}, '{tipo}', 'Manual',
                     {valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},
                     '{data}', '{historico.Replace("'", "''")}', '{criadoEm}')
            ");
        }

        // ── Contas a Pagar demo ───────────────────────────────────────────────
        if (!await ctx.ContasPagar.AnyAsync(c => c.CdFilial == cdFilial))
        {
            var cp = new[]
            {
                (venc: now.AddDays(10).ToString("yyyy-MM-dd"),  valor: 4_200.00m, desc: "Aluguel junho/26",        status: "Aberta"),
                (venc: now.AddDays(15).ToString("yyyy-MM-dd"),  valor: 1_850.00m, desc: "Energia elétrica jun/26", status: "Aberta"),
                (venc: now.AddDays(-5).ToString("yyyy-MM-dd"),  valor:   380.00m, desc: "Internet — fatura vencida", status: "Vencida"),
                (venc: now.AddDays(30).ToString("yyyy-MM-dd"),  valor: 9_800.00m, desc: "Folha de pagamento jun/26", status: "Aberta"),
                (venc: now.AddDays(-12).ToString("yyyy-MM-dd"), valor: 2_100.00m, desc: "Fornecedor Aço Rápido", status: "Vencida"),
            };
            foreach (var (venc, valor, desc, status) in cp)
            {
                await ctx.Database.ExecuteSqlRawAsync($@"
                    INSERT INTO ""ContasPagar""
                        (""CdFilial"", ""Descricao"", ""Valor"", ""Vencimento"", ""Status"", ""CriadoEm"")
                    VALUES
                        ({cdFilial}, '{desc}', {valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},
                         '{venc}', '{status}', '{criadoEm}')
                ");
            }
        }

        // ── Contas a Receber demo ─────────────────────────────────────────────
        if (!await ctx.ContasReceber.AnyAsync(c => c.CdFilial == cdFilial))
        {
            var cr = new[]
            {
                (venc: now.AddDays(7).ToString("yyyy-MM-dd"),   valor: 12_300.00m, desc: "NF 1042 — Metalúrgica SP",   status: "Aberta"),
                (venc: now.AddDays(20).ToString("yyyy-MM-dd"),  valor:  9_850.00m, desc: "NF 1043 — Distribuidora NS", status: "Aberta"),
                (venc: now.AddDays(-3).ToString("yyyy-MM-dd"),  valor:  3_500.00m, desc: "NF 1041 — Assist Técnica",    status: "Vencida"),
                (venc: now.AddDays(45).ToString("yyyy-MM-dd"),  valor: 18_500.00m, desc: "NF 1044 — Construtora Alfa",  status: "Aberta"),
            };
            foreach (var (venc, valor, desc, status) in cr)
            {
                await ctx.Database.ExecuteSqlRawAsync($@"
                    INSERT INTO ""ContasReceber""
                        (""CdFilial"", ""Descricao"", ""Valor"", ""Vencimento"", ""Status"", ""CriadoEm"")
                    VALUES
                        ({cdFilial}, '{desc}', {valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},
                         '{venc}', '{status}', '{criadoEm}')
                ");
            }
        }
    }
}
