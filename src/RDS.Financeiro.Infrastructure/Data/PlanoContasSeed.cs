using Microsoft.EntityFrameworkCore;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Enums;

namespace RDS.Financeiro.Infrastructure.Data;

public static class PlanoContasSeed
{
    /// <summary>
    /// Aplica o plano de contas padrão para a filial informada, caso ainda não exista.
    /// Chamar em Program.cs após app.Build().
    /// </summary>
    public static async Task AplicarAsync(FinanceiroDbContext ctx, int cdFilial = 1)
    {
        if (await ctx.PlanoContas.AnyAsync(x => x.CdFilial == cdFilial))
            return;

        // Nível 1 — Sintéticas raiz
        var ativo        = Add(ctx, cdFilial, "1",   "ATIVO",                    TipoContaPlano.Sintetica);
        var passivo      = Add(ctx, cdFilial, "2",   "PASSIVO",                  TipoContaPlano.Sintetica);
        var receitas     = Add(ctx, cdFilial, "3",   "RECEITAS",                 TipoContaPlano.Sintetica);
        var despesas     = Add(ctx, cdFilial, "4",   "DESPESAS",                 TipoContaPlano.Sintetica);
        var custos       = Add(ctx, cdFilial, "5",   "CUSTOS",                   TipoContaPlano.Sintetica);

        await ctx.SaveChangesAsync();

        // Nível 2 — Sintéticas filhas
        var atCirc       = Add(ctx, cdFilial, "1.1", "Ativo Circulante",         TipoContaPlano.Sintetica,  ativo.Id);
        var atNCirc      = Add(ctx, cdFilial, "1.2", "Ativo Não Circulante",     TipoContaPlano.Sintetica,  ativo.Id);
        var pasCirc      = Add(ctx, cdFilial, "2.1", "Passivo Circulante",       TipoContaPlano.Sintetica,  passivo.Id);
        var pasNCirc     = Add(ctx, cdFilial, "2.2", "Passivo Não Circulante",   TipoContaPlano.Sintetica,  passivo.Id);
        var recOper      = Add(ctx, cdFilial, "3.1", "Receitas Operacionais",    TipoContaPlano.Sintetica,  receitas.Id);
        var despOper     = Add(ctx, cdFilial, "4.1", "Despesas Operacionais",    TipoContaPlano.Sintetica,  despesas.Id);
        var despAdm      = Add(ctx, cdFilial, "4.2", "Despesas Administrativas", TipoContaPlano.Sintetica,  despesas.Id);
        var custoProd    = Add(ctx, cdFilial, "5.1", "Custo de Mercadorias",     TipoContaPlano.Sintetica,  custos.Id);

        await ctx.SaveChangesAsync();

        // Nível 3 — Analíticas (recebem lançamentos)
        Add(ctx, cdFilial, "1.1.1", "Caixa",                         TipoContaPlano.Analitica, atCirc.Id);
        Add(ctx, cdFilial, "1.1.2", "Bancos",                        TipoContaPlano.Analitica, atCirc.Id);
        Add(ctx, cdFilial, "1.1.3", "Contas a Receber",              TipoContaPlano.Analitica, atCirc.Id);
        Add(ctx, cdFilial, "1.1.4", "Estoques",                      TipoContaPlano.Analitica, atCirc.Id);
        Add(ctx, cdFilial, "1.2.1", "Imobilizado",                   TipoContaPlano.Analitica, atNCirc.Id);
        Add(ctx, cdFilial, "2.1.1", "Fornecedores",                  TipoContaPlano.Analitica, pasCirc.Id);
        Add(ctx, cdFilial, "2.1.2", "Contas a Pagar",                TipoContaPlano.Analitica, pasCirc.Id);
        Add(ctx, cdFilial, "2.1.3", "Obrigações Fiscais",            TipoContaPlano.Analitica, pasCirc.Id);
        Add(ctx, cdFilial, "2.1.4", "Obrigações Trabalhistas",       TipoContaPlano.Analitica, pasCirc.Id);
        Add(ctx, cdFilial, "2.2.1", "Empréstimos e Financiamentos",  TipoContaPlano.Analitica, pasNCirc.Id);
        Add(ctx, cdFilial, "3.1.1", "Receita de Vendas",             TipoContaPlano.Analitica, recOper.Id);
        Add(ctx, cdFilial, "3.1.2", "Receita de Serviços",           TipoContaPlano.Analitica, recOper.Id);
        Add(ctx, cdFilial, "4.1.1", "Energia Elétrica",              TipoContaPlano.Analitica, despOper.Id);
        Add(ctx, cdFilial, "4.1.2", "Aluguel",                       TipoContaPlano.Analitica, despOper.Id);
        Add(ctx, cdFilial, "4.1.3", "Telefone e Internet",           TipoContaPlano.Analitica, despOper.Id);
        Add(ctx, cdFilial, "4.2.1", "Salários e Encargos",           TipoContaPlano.Analitica, despAdm.Id);
        Add(ctx, cdFilial, "4.2.2", "Pró-labore",                    TipoContaPlano.Analitica, despAdm.Id);
        Add(ctx, cdFilial, "4.2.3", "Material de Escritório",        TipoContaPlano.Analitica, despAdm.Id);
        Add(ctx, cdFilial, "5.1.1", "CMV — Custo das Mercadorias",   TipoContaPlano.Analitica, custoProd.Id);

        await ctx.SaveChangesAsync();
    }

    private static PlanoContas Add(
        FinanceiroDbContext ctx, int cdFilial, string codigo,
        string descricao, TipoContaPlano tipo, int? paiId = null)
    {
        var conta = new PlanoContas(cdFilial, codigo, descricao, tipo, paiId);
        ctx.PlanoContas.Add(conta);
        return conta;
    }
}
