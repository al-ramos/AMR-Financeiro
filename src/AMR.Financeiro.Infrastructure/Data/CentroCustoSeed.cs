using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Data;

public static class CentroCustoSeed
{
    /// <summary>
    /// Aplica os centros de custo padrão (Card 23.5) para a filial informada, caso ainda não existam:
    /// Administrativo (com TI e RH como filhos), Comercial e Operacional.
    /// Usa SQL direto para contornar comportamento do EF Core 9 + SQLite com chaves geradas
    /// (mesmo padrão do PlanoContasSeed).
    /// </summary>
    public static async Task AplicarAsync(FinanceiroDbContext ctx, int cdFilial = 1)
    {
        if (await ctx.CentrosCusto.AnyAsync(x => x.CdFilial == cdFilial))
            return;

        // Helper: insere via SQL e retorna o Id gerado
        async Task<int> Inserir(string codigo, string descricao, TipoCentroCusto tipo, int? paiId, int nivel)
        {
            var paiParam = paiId.HasValue ? paiId.Value.ToString() : "NULL";
            await ctx.Database.ExecuteSqlRawAsync($@"
                INSERT INTO ""centroscusto"" (""CdFilial"", ""Codigo"", ""Descricao"", ""Tipo"", ""PaiId"", ""Nivel"", ""ResponsavelNome"", ""Ativo"")
                VALUES ({cdFilial}, '{codigo}', '{descricao.Replace("'", "''")}', '{tipo}', {paiParam}, {nivel}, 'A definir', 1)
            ");
            // Lê o Id recém-inserido pelo Codigo (unique por filial)
            return await ctx.CentrosCusto
                .Where(x => x.CdFilial == cdFilial && x.Codigo == codigo)
                .Select(x => x.Id)
                .FirstAsync();
        }

        // Nível 1 — grupos raiz
        var idAdm = await Inserir("01", "Administrativo", TipoCentroCusto.Administrativo, null, 1);
        await Inserir("02", "Comercial",   TipoCentroCusto.Comercial, null, 1);
        await Inserir("03", "Operacional", TipoCentroCusto.Produtivo, null, 1);

        // Nível 2 — áreas de apoio sob o Administrativo
        var idTi = await Inserir("01.01", "TI", TipoCentroCusto.Auxiliar, idAdm, 2);
        await Inserir("01.02", "RH", TipoCentroCusto.Auxiliar, idAdm, 2);

        // Nível 3 — exemplo da hierarquia completa (Administrativo > TI > Infraestrutura)
        await Inserir("01.01.01", "Infraestrutura", TipoCentroCusto.Auxiliar, idTi, 3);
    }
}
