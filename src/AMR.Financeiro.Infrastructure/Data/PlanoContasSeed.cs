using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Data;

public static class PlanoContasSeed
{
    /// <summary>
    /// Aplica o plano de contas padrão para a filial informada, caso ainda não exista.
    /// Usa SQL direto para contornar comportamento do EF Core 9 + SQLite com chaves geradas.
    /// </summary>
    public static async Task AplicarAsync(FinanceiroDbContext ctx, int cdFilial = 1)
    {
        if (await ctx.PlanoContas.AnyAsync(x => x.CdFilial == cdFilial))
            return;

        var now = DateTime.UtcNow.ToString("o");

        // Helper: insere via SQL e retorna o Id gerado
        async Task<int> Inserir(string codigo, string descricao, string tipo, int? paiId)
        {
            var paiParam = paiId.HasValue ? paiId.Value.ToString() : "NULL";
            await ctx.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""PlanoContas"" (""CdFilial"", ""Codigo"", ""Descricao"", ""Tipo"", ""PaiId"", ""Ativo"", ""CriadoEm"")
                VALUES ({cdFilial}, '{codigo}', '{descricao.Replace("'", "''")}', '{tipo}', {paiParam}, 1, '{now}')
            ");
            // Lê o Id recém-inserido pelo Codigo (unique por filial)
            return await ctx.PlanoContas
                .Where(x => x.CdFilial == cdFilial && x.Codigo == codigo)
                .Select(x => x.Id)
                .FirstAsync();
        }

        // Nível 1 — Sintéticas raiz
        var idAtivo    = await Inserir("1", "ATIVO",    nameof(TipoContaPlano.Sintetica), null);
        var idPassivo  = await Inserir("2", "PASSIVO",  nameof(TipoContaPlano.Sintetica), null);
        var idReceitas = await Inserir("3", "RECEITAS", nameof(TipoContaPlano.Sintetica), null);
        var idDespesas = await Inserir("4", "DESPESAS", nameof(TipoContaPlano.Sintetica), null);
        var idCustos   = await Inserir("5", "CUSTOS",   nameof(TipoContaPlano.Sintetica), null);

        // Nível 2 — Sintéticas filhas
        var idAtCirc    = await Inserir("1.1", "Ativo Circulante",         nameof(TipoContaPlano.Sintetica), idAtivo);
        var idAtNCirc   = await Inserir("1.2", "Ativo Não Circulante",     nameof(TipoContaPlano.Sintetica), idAtivo);
        var idPasCirc   = await Inserir("2.1", "Passivo Circulante",       nameof(TipoContaPlano.Sintetica), idPassivo);
        var idPasNCirc  = await Inserir("2.2", "Passivo Não Circulante",   nameof(TipoContaPlano.Sintetica), idPassivo);
        var idRecOper   = await Inserir("3.1", "Receitas Operacionais",    nameof(TipoContaPlano.Sintetica), idReceitas);
        var idDespOper  = await Inserir("4.1", "Despesas Operacionais",    nameof(TipoContaPlano.Sintetica), idDespesas);
        var idDespAdm   = await Inserir("4.2", "Despesas Administrativas", nameof(TipoContaPlano.Sintetica), idDespesas);
        var idCustoProd = await Inserir("5.1", "Custo de Mercadorias",     nameof(TipoContaPlano.Sintetica), idCustos);

        // Nível 3 — Analíticas
        await Inserir("1.1.1", "Caixa",                        nameof(TipoContaPlano.Analitica), idAtCirc);
        await Inserir("1.1.2", "Bancos",                       nameof(TipoContaPlano.Analitica), idAtCirc);
        await Inserir("1.1.3", "Contas a Receber",             nameof(TipoContaPlano.Analitica), idAtCirc);
        await Inserir("1.1.4", "Estoques",                     nameof(TipoContaPlano.Analitica), idAtCirc);
        await Inserir("1.2.1", "Imobilizado",                  nameof(TipoContaPlano.Analitica), idAtNCirc);
        await Inserir("2.1.1", "Fornecedores",                 nameof(TipoContaPlano.Analitica), idPasCirc);
        await Inserir("2.1.2", "Contas a Pagar",               nameof(TipoContaPlano.Analitica), idPasCirc);
        await Inserir("2.1.3", "Obrigações Fiscais",           nameof(TipoContaPlano.Analitica), idPasCirc);
        await Inserir("2.1.4", "Obrigações Trabalhistas",      nameof(TipoContaPlano.Analitica), idPasCirc);
        await Inserir("2.2.1", "Empréstimos e Financiamentos", nameof(TipoContaPlano.Analitica), idPasNCirc);
        await Inserir("3.1.1", "Receita de Vendas",            nameof(TipoContaPlano.Analitica), idRecOper);
        await Inserir("3.1.2", "Receita de Serviços",          nameof(TipoContaPlano.Analitica), idRecOper);
        await Inserir("4.1.1", "Energia Elétrica",             nameof(TipoContaPlano.Analitica), idDespOper);
        await Inserir("4.1.2", "Aluguel",                      nameof(TipoContaPlano.Analitica), idDespOper);
        await Inserir("4.1.3", "Telefone e Internet",          nameof(TipoContaPlano.Analitica), idDespOper);
        await Inserir("4.2.1", "Salários e Encargos",          nameof(TipoContaPlano.Analitica), idDespAdm);
        await Inserir("4.2.2", "Pró-labore",                   nameof(TipoContaPlano.Analitica), idDespAdm);
        await Inserir("4.2.3", "Material de Escritório",       nameof(TipoContaPlano.Analitica), idDespAdm);
        await Inserir("5.1.1", "CMV — Custo das Mercadorias",  nameof(TipoContaPlano.Analitica), idCustoProd);
    }
}
