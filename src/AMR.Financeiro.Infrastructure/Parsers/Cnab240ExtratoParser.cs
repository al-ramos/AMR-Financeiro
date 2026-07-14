using System.Globalization;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Parsers;

/// <summary>
/// Parser de extrato de conta corrente em CNAB 240 (layout FEBRABAN).
/// Lê o header do arquivo (banco / data de geração) e os registros de
/// detalhe com Segmento G (posição 13) — extrato de conta corrente.
/// Posições são zero-based conforme especificado no card 23.3.
/// </summary>
public class Cnab240ExtratoParser : IExtratoParser
{
    public bool Suporta(string conteudo)
    {
        var primeiraLinha = conteudo.Split('\n')[0].TrimEnd('\r');
        return primeiraLinha.Length == 240;
    }

    public ExtratoParseResult Parse(string conteudo)
    {
        var linhas = conteudo
            .Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.Length >= 240)
            .ToList();

        if (linhas.Count == 0)
            throw new FormatException("Arquivo CNAB 240 sem linhas válidas de 240 posições.");

        // Header do arquivo: banco (0-2) e data de geração (143-150, DDMMAAAA)
        var header = linhas[0];
        var banco = header.Substring(0, 3);
        var dataGeracao = TryParseDataCnab(header.Substring(143, 8));
        var contaCorrente = header.Substring(58, 12).Trim(); // conta corrente no header FEBRABAN

        var movimentacoes = new List<MovimentacaoParseItem>();
        foreach (var linha in linhas)
        {
            // Segmento G: código "G" na posição 13
            if (char.ToUpperInvariant(linha[13]) != 'G') continue;

            var data = TryParseDataCnab(linha.Substring(45, 8));
            if (data is null) continue;

            var valorStr = linha.Substring(53, 15).Trim();
            if (!decimal.TryParse(valorStr, NumberStyles.None, CultureInfo.InvariantCulture, out var valorCentavos))
                continue;

            var tipo = char.ToUpperInvariant(linha[44]) == 'C'
                ? TipoMovimentacao.Credito
                : TipoMovimentacao.Debito;

            var documento = linha.Substring(68, 15).Trim();
            var descricao = linha.Substring(83, 40).Trim();

            movimentacoes.Add(new MovimentacaoParseItem(
                data.Value,
                tipo,
                valorCentavos / 100m, // 15 dígitos, últimos 2 são centavos
                string.IsNullOrEmpty(descricao) ? "SEM HISTORICO" : descricao,
                string.IsNullOrEmpty(documento) ? null : documento));
        }

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var dataInicio = movimentacoes.Count > 0 ? movimentacoes.Min(m => m.DataLancamento) : dataGeracao ?? hoje;
        var dataFim = movimentacoes.Count > 0 ? movimentacoes.Max(m => m.DataLancamento) : dataGeracao ?? hoje;

        var totalCreditos = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Credito).Sum(m => m.Valor);
        var totalDebitos = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Debito).Sum(m => m.Valor);

        // O layout de extrato traz saldos em segmentos de trailer não cobertos pelo card;
        // saldo inicial assumido como 0 e saldo final derivado da movimentação do período.
        const decimal saldoInicial = 0m;
        var saldoFinal = saldoInicial + totalCreditos - totalDebitos;

        return new ExtratoParseResult(banco, contaCorrente, dataInicio, dataFim, saldoInicial, saldoFinal, movimentacoes);
    }

    /// <summary>Data CNAB: DDMMAAAA.</summary>
    private static DateOnly? TryParseDataCnab(string valor)
    {
        return DateOnly.TryParseExact(valor.Trim(), "ddMMyyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var data)
            ? data
            : null;
    }
}
