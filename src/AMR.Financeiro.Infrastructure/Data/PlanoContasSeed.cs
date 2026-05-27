using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Data;

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
        var ativo     = Criar(cdFilial, "1", "ATIVO",    TipoContaPlano.Sintetica);
        var passivo   = Criar(cdFilial, "2", "PASSIVO",  TipoContaPlano.Sintetica);
        var receitas  = Criar(cdFilial, "3", "RECEITAS", TipoContaPlano.Sintetica);
        var despesas  = Criar(cdFilial, "4", "DESPESAS", TipoContaPlano.Sintetica);
        var custos    = Criar(cdFilial, "5", "CUSTOS",   TipoContaPlano.Sintetica);

        ctx.PlanoContas.AddRange(ativo, passivo, receitas, despesas, custos);
        await ctx.SaveChangesAsync();
        // Após SaveChangesAsync o EF popula os Ids gerados pelo banco

        // Nível 2 — Sintéticas filhas (usa Id já populado do Nível 1)
        var atCirc    = Criar(cdFilial, "1.1", "Ativo Circulante",         TipoContaPlano.Sintetica,  ativo.Id);
        var atNCirc   = Criar(cdFilial, "1.2", "Ativo Não Circulante",     TipoContaPlano.Sintetica,  ativo.Id);
        var pasCirc   = Criar(cdFilial, "2.1", "Passivo Circulante",       TipoContaPlano.Sintetica,  passivo.Id);
        var pasNCirc  = Criar(cdFilial, "2.2", "Passivo Não Circulante",   TipoContaPlano.Sintetica,  passivo.Id);
        var recOper   = Criar(cdFilial, "3.1", "Receitas Operacionais",    TipoContaPlano.Sintetica,  receitas.Id);
        var despOper  = Criar(cdFilial, "4.1", "Despesas Operacionais",    TipoContaPlano.Sintetica,  despesas.Id);
        var despAdm   = Criar(cdFilial, "4.2", "Despesas Administrativas", TipoContaPlano.Sintetica,  despesas.Id);
        var custoProd = Criar(cdFilial, "5.1", "Custo de Mercadorias",     TipoContaPlano.Sintetica,  custos.Id);

        ctx.PlanoContas.AddRange(atCirc, atNCirc, pasCirc, pasNCirc, recOper, despOper, despAdm, custoProd);
        await ctx.SaveChangesAsync();
        // Ids do Nível 2 agora populados

        // Nível 3 — Analíticas (recebem lançamentos)
        ctx.PlanoContas.AddRange(
            Criar(cdFilial, "1.1.1", "Caixa",                        TipoContaPlano.Analitica, atCirc.Id),
            Criar(cdFilial, "1.1.2", "Bancos",                       TipoContaPlano.Analitica, atCirc.Id),
            Criar(cdFilial, "1.1.3", "Contas a Receber",             TipoContaPlano.Analitica, atCirc.Id),
            Criar(cdFilial, "1.1.4", "Estoques",                     TipoContaPlano.Analitica, atCirc.Id),
            Criar(cdFilial, "1.2.1", "Imobilizado",                  TipoContaPlano.Analitica, atNCirc.Id),
            Criar(cdFilial, "2.1.1", "Fornecedores",                 TipoContaPlano.Analitica, pasCirc.Id),
            Criar(cdFilial, "2.1.2", "Contas a Pagar",               TipoContaPlano.Analitica, pasCirc.Id),
            Criar(cdFilial, "2.1.3", "Obrigações Fiscais",           TipoContaPlano.Analitica, pasCirc.Id),
            Criar(cdFilial, "2.1.4", "Obrigações Trabalhistas",      TipoContaPlano.Analitica, pasCirc.Id),
            Criar(cdFilial, "2.2.1", "Empréstimos e Financiamentos", TipoContaPlano.Analitica, pasNCirc.Id),
            Criar(cdFilial, "3.1.1", "Receita de Vendas",            TipoContaPlano.Analitica, recOper.Id),
            Criar(cdFilial, "3.1.2", "Receita de Serviços",          TipoContaPlano.Analitica, recOper.Id),
            Criar(cdFilial, "4.1.1", "Energia Elétrica",             TipoContaPlano.Analitica, despOper.Id),
            Criar(cdFilial, "4.1.2", "Aluguel",                      TipoContaPlano.Analitica, despOper.Id),
            Criar(cdFilial, "4.1.3", "Telefone e Internet",          TipoContaPlano.Analitica, despOper.Id),
            Criar(cdFilial, "4.2.1", "Salários e Encargos",          TipoContaPlano.Analitica, despAdm.Id),
            Criar(cdFilial, "4.2.2", "Pró-labore",                   TipoContaPlano.Analitica, despAdm.Id),
            Criar(cdFilial, "4.2.3", "Material de Escritório",       TipoContaPlano.Analitica, despAdm.Id),
            Criar(cdFilial, "5.1.1", "CMV — Custo das Mercadorias",  TipoContaPlano.Analitica, custoProd.Id)
        );

        await ctx.SaveChangesAsync();
    }

    private static PlanoContas Criar(
        int cdFilial, string codigo,
        string descricao, TipoContaPlano tipo, int? paiId = null)
    {
        return new PlanoContas(cdFilial, codigo, descricao, tipo, paiId);
    }
}
