using System.Globalization;
using System.Text.RegularExpressions;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Parsers;

/// <summary>
/// Parser de extratos OFX (Open Financial Exchange) no formato SGML legado —
/// tags sem fechamento obrigatório — e também no formato XML (os mesmos regex funcionam).
/// </summary>
public class OfxParser : IExtratoParser
{
    public bool Suporta(string conteudo) =>
        conteudo.Contains("OFXHEADER:", StringComparison.OrdinalIgnoreCase) ||
        conteudo.Contains("<OFX>", StringComparison.OrdinalIgnoreCase);

    public ExtratoParseResult Parse(string conteudo)
    {
        var banco = ExtrairCampo(conteudo, "BANKID") ?? ExtrairCampo(conteudo, "ORG") ?? "DESCONHECIDO";
        var contaCorrente = ExtrairCampo(conteudo, "ACCTID") ?? string.Empty;

        var movimentacoes = ParseMovimentacoes(conteudo);

        var totalCreditos = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Credito).Sum(m => m.Valor);
        var totalDebitos = movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Debito).Sum(m => m.Valor);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var dtStart = ExtrairCampo(conteudo, "DTSTART");
        var dtEnd = ExtrairCampo(conteudo, "DTEND");
        var dataInicio = dtStart is not null
            ? ParseDataOfx(dtStart)
            : movimentacoes.Count > 0 ? movimentacoes.Min(m => m.DataLancamento) : hoje;
        var dataFim = dtEnd is not null
            ? ParseDataOfx(dtEnd)
            : movimentacoes.Count > 0 ? movimentacoes.Max(m => m.DataLancamento) : hoje;

        // BALAMT (LEDGERBAL) = saldo final; saldo inicial é derivado dos totais do período.
        var balamt = ExtrairCampo(conteudo, "BALAMT");
        var saldoFinal = balamt is not null ? ParseDecimalOfx(balamt) : totalCreditos - totalDebitos;
        var saldoInicial = saldoFinal - totalCreditos + totalDebitos;

        return new ExtratoParseResult(banco, contaCorrente, dataInicio, dataFim, saldoInicial, saldoFinal, movimentacoes);
    }

    private static List<MovimentacaoParseItem> ParseMovimentacoes(string conteudo)
    {
        var movimentacoes = new List<MovimentacaoParseItem>();

        // No SGML legado o </STMTTRN> pode não existir: cada bloco vai até o próximo <STMTTRN>.
        var blocos = Regex.Split(conteudo, "<STMTTRN>", RegexOptions.IgnoreCase);
        for (var i = 1; i < blocos.Length; i++)
        {
            var bloco = blocos[i];
            var fim = bloco.IndexOf("</STMTTRN>", StringComparison.OrdinalIgnoreCase);
            if (fim >= 0) bloco = bloco[..fim];

            var dataStr = ExtrairCampo(bloco, "DTPOSTED");
            var valorStr = ExtrairCampo(bloco, "TRNAMT");
            if (dataStr is null || valorStr is null) continue;

            var valor = ParseDecimalOfx(valorStr);
            var tipoStr = ExtrairCampo(bloco, "TRNTYPE") ?? string.Empty;
            var tipo = tipoStr.Equals("CREDIT", StringComparison.OrdinalIgnoreCase)
                ? TipoMovimentacao.Credito
                : TipoMovimentacao.Debito;

            // No SGML antigo o sinal do TRNAMT prevalece: negativo inverte o tipo.
            if (valor < 0)
            {
                tipo = tipo == TipoMovimentacao.Credito ? TipoMovimentacao.Debito : TipoMovimentacao.Credito;
                valor = Math.Abs(valor);
            }

            var descricao = ExtrairCampo(bloco, "MEMO") ?? ExtrairCampo(bloco, "NAME") ?? "SEM DESCRICAO";
            var codigoDoc = ExtrairCampo(bloco, "FITID") ?? ExtrairCampo(bloco, "CHECKNUM");

            movimentacoes.Add(new MovimentacaoParseItem(
                ParseDataOfx(dataStr), tipo, valor, descricao, codigoDoc));
        }

        return movimentacoes;
    }

    /// <summary>Extrai o valor de uma tag SGML/XML: tudo após &lt;TAG&gt; até o próximo '&lt;' ou quebra de linha.</summary>
    private static string? ExtrairCampo(string conteudo, string tag)
    {
        var match = Regex.Match(conteudo, $@"<{tag}>\s*([^<\r\n]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    /// <summary>Data OFX: YYYYMMDD ou YYYYMMDDHHMMSS (com possível timezone [-3:BRT]).</summary>
    private static DateOnly ParseDataOfx(string valor)
    {
        var digitos = new string(valor.Where(char.IsDigit).Take(8).ToArray());
        if (digitos.Length < 8)
            throw new FormatException($"Data OFX inválida: '{valor}'.");
        return DateOnly.ParseExact(digitos, "yyyyMMdd", CultureInfo.InvariantCulture);
    }

    private static decimal ParseDecimalOfx(string valor)
    {
        // OFX usa ponto decimal, mas alguns bancos brasileiros emitem vírgula.
        var normalizado = valor.Trim().Replace(",", ".");
        return decimal.Parse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture);
    }
}
