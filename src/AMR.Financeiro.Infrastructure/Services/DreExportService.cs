using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Stub profissional de exportação da DRE (Card 23.4), no mesmo estilo do NFeService (Card 23.1).
///
/// Gera arquivos REAIS e abríveis sem dependências externas:
/// - Excel: XML Spreadsheet 2003 (SpreadsheetML), que o Excel/LibreOffice abre nativamente;
/// - PDF: PDF 1.4 mínimo escrito à mão, com as linhas da DRE em fonte Courier.
///
/// A implementação definitiva usará ClosedXML (xlsx real, com estilos/merge/freeze panes)
/// e QuestPDF (layout profissional com header/footer e paginação). Os pontos de integração
/// estão comentados em cada método.
/// </summary>
public class DreExportService(ILogger<DreExportService> logger) : IDreExportService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public Task<DreExportFile> GerarExcelAsync(DreResult dre, int cdFilial, CancellationToken ct = default)
    {
        // ------------------------------------------------------------------
        // FLUXO REAL COM ClosedXML (referência para a implementação definitiva):
        //
        //   using var wb = new XLWorkbook();
        //   var ws = wb.Worksheets.Add($"DRE {dre.Periodo}");
        //   ws.Cell(1, 1).Value = $"DRE — Demonstração de Resultado — {dre.Periodo}";
        //   ws.Range(1, 1, 1, 6).Merge().Style.Font.SetBold().Font.SetFontSize(14);
        //   // header (linha 3): Linha | Atual | Mês Anterior | Mesmo Mês Ano Ant. | Var. Mês % | Var. Ano %
        //   foreach (var linha in dre.Linhas) { ... ws.Cell(r, 2).Style.NumberFormat.Format = "#,##0.00"; }
        //   // subtotais (linha.EhSubtotal) com Font.Bold e fundo cinza; margens no rodapé
        //   ws.Columns().AdjustToContents();
        //   using var ms = new MemoryStream(); wb.SaveAs(ms);
        //   → ContentType "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", extensão .xlsx
        // ------------------------------------------------------------------

        logger.LogInformation(
            "SIMULAÇÃO Export DRE Excel (SpreadsheetML): filial {CdFilial}, período {Periodo}, {Linhas} linhas",
            cdFilial, dre.Periodo, dre.Linhas.Count);

        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        sb.AppendLine("""<?mso-application progid="Excel.Sheet"?>""");
        sb.AppendLine("""<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">""");
        sb.AppendLine(""" <Styles>""");
        sb.AppendLine("""  <Style ss:ID="titulo"><Font ss:Bold="1" ss:Size="14"/></Style>""");
        sb.AppendLine("""  <Style ss:ID="header"><Font ss:Bold="1"/><Interior ss:Color="#D9D9D9" ss:Pattern="Solid"/></Style>""");
        sb.AppendLine("""  <Style ss:ID="subtotal"><Font ss:Bold="1"/></Style>""");
        sb.AppendLine("""  <Style ss:ID="valor"><NumberFormat ss:Format="#,##0.00"/></Style>""");
        sb.AppendLine("""  <Style ss:ID="valorSub"><Font ss:Bold="1"/><NumberFormat ss:Format="#,##0.00"/></Style>""");
        sb.AppendLine(""" </Styles>""");
        sb.AppendLine($""" <Worksheet ss:Name="DRE {XmlEscape(dre.Periodo.Replace('/', '-'))}">""");
        sb.AppendLine("""  <Table>""");
        sb.AppendLine("""   <Column ss:Width="260"/><Column ss:Width="90"/><Column ss:Width="90"/><Column ss:Width="110"/><Column ss:Width="70"/><Column ss:Width="70"/>""");

        sb.AppendLine("   <Row>");
        sb.AppendLine($"""    <Cell ss:StyleID="titulo"><Data ss:Type="String">DRE — Demonstração de Resultado — {XmlEscape(dre.Periodo)} (Filial {cdFilial})</Data></Cell>""");
        sb.AppendLine("   </Row>");
        sb.AppendLine("   <Row/>");

        sb.AppendLine("   <Row>");
        foreach (var h in new[] { "Linha", "Atual", "Mês Anterior", "Mesmo Mês Ano Anterior", "Var. Mês %", "Var. Ano %" })
            sb.AppendLine($"""    <Cell ss:StyleID="header"><Data ss:Type="String">{XmlEscape(h)}</Data></Cell>""");
        sb.AppendLine("   </Row>");

        foreach (var linha in dre.Linhas)
        {
            var textoStyle = linha.EhSubtotal ? "subtotal" : "Default";
            var valorStyle = linha.EhSubtotal ? "valorSub" : "valor";

            sb.AppendLine("   <Row>");
            sb.AppendLine($"""    <Cell ss:StyleID="{textoStyle}"><Data ss:Type="String">{XmlEscape(linha.Descricao)}</Data></Cell>""");
            foreach (var valor in new[] { linha.ValorAtual, linha.ValorPeriodoAnterior, linha.ValorMesmoMesAnoAnterior, linha.VariacaoMes, linha.VariacaoAno })
                sb.AppendLine($"""    <Cell ss:StyleID="{valorStyle}"><Data ss:Type="Number">{valor.ToString(CultureInfo.InvariantCulture)}</Data></Cell>""");
            sb.AppendLine("   </Row>");

            // Detalhamento por conta analítica (indentado sob a linha do grupo)
            foreach (var conta in linha.Contas)
            {
                sb.AppendLine("   <Row>");
                sb.AppendLine($"""    <Cell><Data ss:Type="String">    {XmlEscape(conta.Codigo)} — {XmlEscape(conta.Descricao)}</Data></Cell>""");
                sb.AppendLine($"""    <Cell ss:StyleID="valor"><Data ss:Type="Number">{conta.Valor.ToString(CultureInfo.InvariantCulture)}</Data></Cell>""");
                sb.AppendLine("   </Row>");
            }
        }

        sb.AppendLine("   <Row/>");
        AppendMargemRow(sb, "Margem Bruta %", dre.MargemBruta);
        AppendMargemRow(sb, "Margem Operacional %", dre.MargemOperacional);
        AppendMargemRow(sb, "Margem Líquida %", dre.MargemLiquida);

        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");
        sb.AppendLine("</Workbook>");

        var arquivo = new DreExportFile(
            Conteudo: Encoding.UTF8.GetBytes(sb.ToString()),
            ContentType: "application/vnd.ms-excel",
            NomeArquivo: $"dre_{cdFilial}_{dre.Periodo.Replace('/', '-')}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xls");

        return Task.FromResult(arquivo);
    }

    public Task<DreExportFile> GerarPdfAsync(DreResult dre, int cdFilial, CancellationToken ct = default)
    {
        // ------------------------------------------------------------------
        // FLUXO REAL COM QuestPDF (referência para a implementação definitiva):
        //
        //   QuestPDF.Settings.License = LicenseType.Community;
        //   var pdf = Document.Create(container => container.Page(page =>
        //   {
        //       page.Size(PageSizes.A4);
        //       page.Header().Text($"DRE — {dre.Periodo}").Bold().FontSize(16);
        //       page.Content().Table(table =>
        //       {
        //           table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(); ... });
        //           // header repetido por página via table.Header(...); linhas EhSubtotal em Bold
        //           // com fundo cinza; valores com formato "N2" pt-BR alinhados à direita
        //       });
        //       page.Footer().AlignRight().Text(t => { t.CurrentPageNumber(); t.Span(" / "); t.TotalPages(); });
        //   })).GeneratePdf();
        // ------------------------------------------------------------------

        logger.LogInformation(
            "SIMULAÇÃO Export DRE PDF (PDF 1.4 mínimo): filial {CdFilial}, período {Periodo}, {Linhas} linhas",
            cdFilial, dre.Periodo, dre.Linhas.Count);

        var texto = new StringBuilder();
        texto.AppendLine($"DRE - DEMONSTRACAO DE RESULTADO - {dre.Periodo} (FILIAL {cdFilial})");
        texto.AppendLine(new string('=', 92));
        texto.AppendLine($"{"LINHA",-46}{"ATUAL",14}{"MES ANT.",14}{"ANO ANT.",14}");
        texto.AppendLine(new string('-', 92));

        foreach (var linha in dre.Linhas)
        {
            texto.AppendLine(
                $"{Truncar(RemoverAcentos(linha.Descricao), 46),-46}" +
                $"{linha.ValorAtual.ToString("N2", PtBr),14}" +
                $"{linha.ValorPeriodoAnterior.ToString("N2", PtBr),14}" +
                $"{linha.ValorMesmoMesAnoAnterior.ToString("N2", PtBr),14}");
        }

        texto.AppendLine(new string('-', 92));
        texto.AppendLine($"MARGEM BRUTA: {dre.MargemBruta.ToString("N2", PtBr)}%   " +
                         $"MARGEM OPERACIONAL: {dre.MargemOperacional.ToString("N2", PtBr)}%   " +
                         $"MARGEM LIQUIDA: {dre.MargemLiquida.ToString("N2", PtBr)}%");
        texto.AppendLine();
        texto.AppendLine($"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC - AMR Financeiro (stub - QuestPDF pendente)");

        var pdfBytes = GerarPdfMinimo(texto.ToString());

        var arquivo = new DreExportFile(
            Conteudo: pdfBytes,
            ContentType: "application/pdf",
            NomeArquivo: $"dre_{cdFilial}_{dre.Periodo.Replace('/', '-')}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");

        return Task.FromResult(arquivo);
    }

    private static void AppendMargemRow(StringBuilder sb, string nome, decimal valor)
    {
        sb.AppendLine("   <Row>");
        sb.AppendLine($"""    <Cell ss:StyleID="subtotal"><Data ss:Type="String">{XmlEscape(nome)}</Data></Cell>""");
        sb.AppendLine($"""    <Cell ss:StyleID="valorSub"><Data ss:Type="Number">{valor.ToString(CultureInfo.InvariantCulture)}</Data></Cell>""");
        sb.AppendLine("   </Row>");
    }

    /// <summary>PDF 1.4 mínimo com uma página A4 e texto em Courier, uma linha por Td.</summary>
    private static byte[] GerarPdfMinimo(string conteudo)
    {
        var stream = new StringBuilder();
        stream.Append("BT /F1 8 Tf 40 800 Td 10 TL ");
        foreach (var linha in conteudo.Replace("\r", "").Split('\n'))
            stream.Append($"({EscaparPdf(linha)}) Tj T* ");
        stream.Append("ET");

        var streamStr = stream.ToString();

        var pdf = new StringBuilder();
        pdf.AppendLine("%PDF-1.4");
        pdf.AppendLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");
        pdf.AppendLine("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj");
        pdf.AppendLine("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj");
        pdf.AppendLine($"4 0 obj << /Length {Encoding.ASCII.GetByteCount(streamStr)} >> stream");
        pdf.AppendLine(streamStr);
        pdf.AppendLine("endstream endobj");
        pdf.AppendLine("5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Courier >> endobj");
        pdf.AppendLine("trailer << /Root 1 0 R >>");
        pdf.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string EscaparPdf(string texto) =>
        RemoverAcentos(texto).Replace("\\", @"\\").Replace("(", @"\(").Replace(")", @"\)");

    /// <summary>Normaliza acentos para ASCII (fontes standard do PDF não cobrem UTF-8 sem embedding).</summary>
    private static string RemoverAcentos(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var c in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != UnicodeCategory.NonSpacingMark)
                sb.Append(c <= 127 ? c : '?');
        }
        return sb.ToString();
    }

    private static string Truncar(string texto, int max) =>
        texto.Length <= max ? texto : texto[..(max - 1)] + ".";

    private static string XmlEscape(string texto) =>
        texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
