using System.Globalization;
using System.Text;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Implementação STUB de IBoletoService (mesma abordagem do NFeService em homologação).
/// Gera linha digitável, código de barras, PDF simplificado e arquivos CNAB 240/400
/// sem depender do pacote BoletoNet. Quando o BoletoNet for instalado, apenas esta
/// classe precisa ser substituída — as assinaturas de IBoletoService permanecem.
/// </summary>
public class BoletoService : IBoletoService
{
    private static readonly DateOnly DataBaseFatorVencimento = new(1997, 10, 7);

    public Task<BoletoGeradoResult> GerarAsync(
        BancoBoleto banco, int nossoNumero,
        string sacadoNome, string sacadoCpfCnpj, string sacadoEndereco,
        decimal valor, DateOnly vencimento,
        string instrucao1, string instrucao2,
        CancellationToken ct = default)
    {
        var nossoNumeroFormatado = FormatarNossoNumero(banco, nossoNumero);
        var codigoBarras = GerarCodigoBarras(banco, nossoNumeroFormatado, valor, vencimento);
        var linhaDigitavel = GerarLinhaDigitavel(codigoBarras);
        var pdfBase64 = GerarPdfBase64(banco, nossoNumeroFormatado, linhaDigitavel, codigoBarras,
            sacadoNome, sacadoCpfCnpj, sacadoEndereco, valor, vencimento, instrucao1, instrucao2);

        return Task.FromResult(new BoletoGeradoResult(
            nossoNumeroFormatado, linhaDigitavel, codigoBarras, pdfBase64));
    }

    public Task<RemessaGeradaResult> GerarRemessaAsync(
        BancoBoleto banco, TipoCnab tipo,
        List<Boleto> boletos, CancellationToken ct = default)
    {
        var agora = DateTime.UtcNow;
        var nomeArquivo = tipo == TipoCnab.CNAB240
            ? $"CB240_{agora:yyyyMMdd_HHmmss}.txt"
            : $"CB400_{agora:yyyyMMdd_HHmmss}.txt";

        var conteudo = tipo == TipoCnab.CNAB240
            ? GerarCnab240(banco, boletos, agora)
            : GerarCnab400(banco, boletos, agora);

        var valorTotal = boletos.Sum(b => b.Valor);
        var cnabBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(conteudo));

        return Task.FromResult(new RemessaGeradaResult(
            nomeArquivo, cnabBase64, conteudo, boletos.Count, valorTotal));
    }

    public Task<List<RetornoProcessadoItem>> ProcessarRetornoAsync(
        string conteudoArquivo, BancoBoleto banco, CancellationToken ct = default)
    {
        var itens = new List<RetornoProcessadoItem>();

        var linhas = conteudoArquivo
            .Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        foreach (var linha in linhas)
        {
            try
            {
                if (linha.Length >= 400)
                {
                    var item = ProcessarLinhaRetorno400(linha);
                    if (item is not null) itens.Add(item);
                }
                else if (linha.Length >= 240)
                {
                    var item = ProcessarLinhaRetorno240(linha);
                    if (item is not null) itens.Add(item);
                }
                // Linhas menores (ex.: arquivos truncados) são ignoradas.
            }
            catch (Exception ex)
            {
                itens.Add(new RetornoProcessadoItem(
                    string.Empty, DateOnly.MinValue, 0m, false,
                    $"Falha ao interpretar linha do retorno: {ex.Message}"));
            }
        }

        return Task.FromResult(itens);
    }

    // ------------------------------------------------------------------
    // Retorno — parsing
    // ------------------------------------------------------------------

    /// <summary>
    /// CNAB 240 — layout do stub (posições 1-based):
    /// ocorrência pos 15 (2), nosso número pos 35 (20), valor pago pos 77 (15, centavos),
    /// data pagamento pos 145 (8, YYYYMMDD). Somente registros de detalhe (tipo '3' na pos 8).
    /// </summary>
    private static RetornoProcessadoItem? ProcessarLinhaRetorno240(string linha)
    {
        // Ignora header (tipo '0'/'1') e trailer (tipo '5'/'9') — só detalhe interessa.
        if (linha[7] != '3') return null;

        var ocorrencia = linha.Substring(14, 2);
        var nossoNumero = linha.Substring(34, 20).Trim();
        var valorPago = decimal.Parse(linha.Substring(76, 15), CultureInfo.InvariantCulture) / 100m;
        var dataPagamento = DateOnly.ParseExact(linha.Substring(144, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

        // "06" = liquidação normal.
        return ocorrencia == "06"
            ? new RetornoProcessadoItem(nossoNumero, dataPagamento, valorPago, true, null)
            : new RetornoProcessadoItem(nossoNumero, dataPagamento, valorPago, false, null);
    }

    /// <summary>
    /// CNAB 400 — layout do stub (posições 1-based):
    /// registro detalhe tipo '1' na pos 1, nosso número pos 63 (20),
    /// ocorrência pos 109 (2), data pagamento pos 111 (6, DDMMYY), valor pago pos 254 (13, centavos).
    /// </summary>
    private static RetornoProcessadoItem? ProcessarLinhaRetorno400(string linha)
    {
        if (linha[0] != '1') return null;

        var nossoNumero = linha.Substring(62, 20).Trim();
        var ocorrencia = linha.Substring(108, 2);
        var dataPagamento = DateOnly.ParseExact(linha.Substring(110, 6), "ddMMyy", CultureInfo.InvariantCulture);
        var valorPago = decimal.Parse(linha.Substring(253, 13), CultureInfo.InvariantCulture) / 100m;

        return ocorrencia == "06"
            ? new RetornoProcessadoItem(nossoNumero, dataPagamento, valorPago, true, null)
            : new RetornoProcessadoItem(nossoNumero, dataPagamento, valorPago, false, null);
    }

    // ------------------------------------------------------------------
    // Remessa — geração
    // ------------------------------------------------------------------

    private static string GerarCnab240(BancoBoleto banco, List<Boleto> boletos, DateTime agora)
    {
        var sb = new StringBuilder();
        var codBanco = ((int)banco).ToString("000");

        // Header de arquivo (tipo de registro '0' na pos 8)
        var header = new StringBuilder(new string(' ', 240));
        Escrever(header, 1, codBanco);
        Escrever(header, 4, "0000");        // lote de serviço
        Escrever(header, 8, "0");           // tipo de registro: header de arquivo
        Escrever(header, 143, "1");         // código remessa/retorno: 1 = remessa
        Escrever(header, 144, agora.ToString("ddMMyyyy"));
        Escrever(header, 152, agora.ToString("HHmmss"));
        Escrever(header, 158, "000001");    // sequência do arquivo (NSA)
        sb.AppendLine(header.ToString());

        // Detalhes — segmento P simplificado (tipo de registro '3' na pos 8)
        var seq = 0;
        foreach (var b in boletos)
        {
            seq++;
            var det = new StringBuilder(new string(' ', 240));
            Escrever(det, 1, codBanco);
            Escrever(det, 4, "0001");                                       // lote
            Escrever(det, 8, "3");                                          // tipo de registro: detalhe
            Escrever(det, 9, seq.ToString("00000"));                        // sequencial do registro
            Escrever(det, 14, "P");                                         // segmento
            Escrever(det, 15, "01");                                        // código de movimento: 01 = entrada de título
            Escrever(det, 35, b.NossoNumero.PadRight(20));                  // nosso número (20)
            Escrever(det, 77, ((long)(b.Valor * 100m)).ToString("000000000000000")); // valor (15, centavos)
            Escrever(det, 145, b.Vencimento.ToString("yyyyMMdd"));          // vencimento (8)
            Escrever(det, 170, TruncarPad(b.SacadoNome, 40));               // sacado
            Escrever(det, 210, TruncarPad(b.SacadoCpfCnpj, 14));            // CPF/CNPJ sacado
            sb.AppendLine(det.ToString());
        }

        // Trailer de arquivo (tipo de registro '9' na pos 8)
        var trailer = new StringBuilder(new string(' ', 240));
        Escrever(trailer, 1, codBanco);
        Escrever(trailer, 4, "9999");
        Escrever(trailer, 8, "9");
        Escrever(trailer, 18, (boletos.Count + 2).ToString("000000"));      // total de registros do arquivo
        sb.AppendLine(trailer.ToString());

        return sb.ToString();
    }

    private static string GerarCnab400(BancoBoleto banco, List<Boleto> boletos, DateTime agora)
    {
        var sb = new StringBuilder();
        var codBanco = ((int)banco).ToString("000");

        // Header (tipo '0' na pos 1)
        var header = new StringBuilder(new string(' ', 400));
        Escrever(header, 1, "0");
        Escrever(header, 2, "1");                       // 1 = remessa
        Escrever(header, 3, "REMESSA");
        Escrever(header, 77, codBanco);
        Escrever(header, 95, agora.ToString("ddMMyy"));
        Escrever(header, 395, "000001");                // sequencial
        sb.AppendLine(header.ToString());

        // Detalhes (tipo '1' na pos 1)
        var seq = 1;
        foreach (var b in boletos)
        {
            seq++;
            var det = new StringBuilder(new string(' ', 400));
            Escrever(det, 1, "1");
            Escrever(det, 63, b.NossoNumero.PadRight(20));                  // nosso número (20)
            Escrever(det, 109, "01");                                       // ocorrência: 01 = remessa
            Escrever(det, 121, b.Vencimento.ToString("ddMMyy"));            // vencimento (6)
            Escrever(det, 127, ((long)(b.Valor * 100m)).ToString("0000000000000")); // valor (13, centavos)
            Escrever(det, 235, TruncarPad(b.SacadoCpfCnpj, 14));            // CPF/CNPJ sacado
            Escrever(det, 249, TruncarPad(b.SacadoNome, 40));               // nome sacado
            Escrever(det, 395, seq.ToString("000000"));                     // sequencial
            sb.AppendLine(det.ToString());
        }

        // Trailer (tipo '9' na pos 1)
        var trailer = new StringBuilder(new string(' ', 400));
        Escrever(trailer, 1, "9");
        Escrever(trailer, 395, (seq + 1).ToString("000000"));
        sb.AppendLine(trailer.ToString());

        return sb.ToString();
    }

    /// <summary>Escreve <paramref name="valor"/> na posição 1-based <paramref name="pos"/> do buffer de tamanho fixo.</summary>
    private static void Escrever(StringBuilder buffer, int pos, string valor)
    {
        var inicio = pos - 1;
        var len = Math.Min(valor.Length, buffer.Length - inicio);
        for (var i = 0; i < len; i++)
            buffer[inicio + i] = valor[i];
    }

    private static string TruncarPad(string valor, int tamanho) =>
        (valor.Length > tamanho ? valor[..tamanho] : valor).PadRight(tamanho);

    // ------------------------------------------------------------------
    // Boleto — nosso número, código de barras, linha digitável, PDF
    // ------------------------------------------------------------------

    private static string FormatarNossoNumero(BancoBoleto banco, int nossoNumero)
    {
        // Tamanho do campo numérico varia por banco; DV mod-11 anexado ao final.
        var corpo = banco switch
        {
            BancoBoleto.Itau => nossoNumero.ToString("00000000"),        // 8 dígitos
            BancoBoleto.Bradesco => nossoNumero.ToString("00000000000"), // 11 dígitos
            BancoBoleto.BancoDoBrasil => nossoNumero.ToString("0000000000"), // 10 dígitos
            BancoBoleto.Santander => nossoNumero.ToString("000000000000"),   // 12 dígitos
            _ => nossoNumero.ToString("0000000000")
        };
        return corpo + DigitoMod11(corpo);
    }

    private static string GerarCodigoBarras(BancoBoleto banco, string nossoNumero, decimal valor, DateOnly vencimento)
    {
        var codBanco = ((int)banco).ToString("000");
        const string moeda = "9";
        var fator = CalcularFatorVencimento(vencimento).ToString("0000");
        var valorStr = ((long)(valor * 100m)).ToString("0000000000");

        // Campo livre (25): nosso número + agência/conta fictícias do stub, zero-fill.
        var campoLivre = (nossoNumero + "1234567890").PadRight(25, '0')[..25];

        var semDv = codBanco + moeda + fator + valorStr + campoLivre; // 43 dígitos
        var dvGeral = DigitoMod11(semDv);

        // Código de barras (44): banco(3) moeda(1) DV(1) fator(4) valor(10) campo livre(25)
        return codBanco + moeda + dvGeral + fator + valorStr + campoLivre;
    }

    /// <summary>Linha digitável (47 dígitos) derivada do código de barras, com DVs mod-10 por campo.</summary>
    private static string GerarLinhaDigitavel(string codigoBarras)
    {
        var banco = codigoBarras[..3];
        var moeda = codigoBarras[3..4];
        var dvGeral = codigoBarras[4..5];
        var fatorEValor = codigoBarras[5..19];   // fator (4) + valor (10)
        var campoLivre = codigoBarras[19..44];   // 25 dígitos

        var campo1 = banco + moeda + campoLivre[..5];
        var campo2 = campoLivre[5..15];
        var campo3 = campoLivre[15..25];

        return campo1 + DigitoMod10(campo1)   // 10 dígitos
             + campo2 + DigitoMod10(campo2)   // 11 dígitos
             + campo3 + DigitoMod10(campo3)   // 11 dígitos
             + dvGeral                        // 1 dígito
             + fatorEValor;                   // 14 dígitos → total 47
    }

    private static int CalcularFatorVencimento(DateOnly vencimento)
    {
        var dias = vencimento.DayNumber - DataBaseFatorVencimento.DayNumber;
        if (dias <= 0) return 0;
        // Regra FEBRABAN de reinício do fator após 9999 (22/02/2025): volta para 1000.
        return dias <= 9999 ? dias : ((dias - 1000) % 9000) + 1000;
    }

    private static char DigitoMod10(string numero)
    {
        var soma = 0;
        var peso = 2;
        for (var i = numero.Length - 1; i >= 0; i--)
        {
            var produto = (numero[i] - '0') * peso;
            soma += produto > 9 ? produto - 9 : produto;
            peso = peso == 2 ? 1 : 2;
        }
        var resto = soma % 10;
        return resto == 0 ? '0' : (char)('0' + (10 - resto));
    }

    private static char DigitoMod11(string numero)
    {
        var soma = 0;
        var peso = 2;
        for (var i = numero.Length - 1; i >= 0; i--)
        {
            soma += (numero[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        var resto = soma % 11;
        var dv = 11 - resto;
        return dv is 0 or 10 or 11 ? '1' : (char)('0' + dv);
    }

    private static string GerarPdfBase64(
        BancoBoleto banco, string nossoNumero, string linhaDigitavel, string codigoBarras,
        string sacadoNome, string sacadoCpfCnpj, string sacadoEndereco,
        decimal valor, DateOnly vencimento, string instrucao1, string instrucao2)
    {
        // Stub: documento textual em base64 com os dados do boleto.
        // Será substituído pelo PDF real quando o BoletoNet for instalado.
        var texto = new StringBuilder()
            .AppendLine("===== BOLETO BANCÁRIO (HOMOLOGAÇÃO — SEM VALOR FISCAL) =====")
            .AppendLine($"Banco............: {banco} ({(int)banco:000})")
            .AppendLine($"Nosso Número.....: {nossoNumero}")
            .AppendLine($"Linha Digitável..: {linhaDigitavel}")
            .AppendLine($"Código de Barras.: {codigoBarras}")
            .AppendLine($"Valor............: {valor.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"))}")
            .AppendLine($"Vencimento.......: {vencimento:dd/MM/yyyy}")
            .AppendLine($"Sacado...........: {sacadoNome}")
            .AppendLine($"CPF/CNPJ.........: {sacadoCpfCnpj}")
            .AppendLine($"Endereço.........: {sacadoEndereco}")
            .AppendLine($"Instrução 1......: {instrucao1}")
            .AppendLine($"Instrução 2......: {instrucao2}")
            .ToString();

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(texto));
    }
}
