using System.Globalization;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Exportação da DRE (Card 23.4):
/// - Excel: xlsx real via ClosedXML (estilos, merge, freeze panes, autofit);
/// - PDF: QuestPDF com header/footer, paginação e linhas de subtotal destacadas.
/// </summary>
public class DreExportService(ILogger<DreExportService> logger) : IDreExportService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    static DreExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<DreExportFile> GerarExcelAsync(DreResult dre, int cdFilial, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Export DRE Excel (ClosedXML): filial {CdFilial}, período {Periodo}, {Linhas} linhas",
            cdFilial, dre.Periodo, dre.Linhas.Count);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add($"DRE {dre.Periodo.Replace('/', '-')}");

        ws.Cell(1, 1).Value = $"DRE — Demonstração de Resultado — {dre.Periodo} (Filial {cdFilial})";
        ws.Range(1, 1, 1, 6).Merge().Style
            .Font.SetBold()
            .Font.SetFontSize(14);

        var headers = new[] { "Linha", "Atual", "Mês Anterior", "Mesmo Mês Ano Anterior", "Var. Mês %", "Var. Ano %" };
        const int headerRow = 3;
        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.SetBold();
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9D9D9"));
        }

        var r = headerRow + 1;
        foreach (var linha in dre.Linhas)
        {
            ws.Cell(r, 1).Value = linha.Descricao;
            ws.Cell(r, 2).Value = linha.ValorAtual;
            ws.Cell(r, 3).Value = linha.ValorPeriodoAnterior;
            ws.Cell(r, 4).Value = linha.ValorMesmoMesAnoAnterior;
            ws.Cell(r, 5).Value = linha.VariacaoMes;
            ws.Cell(r, 6).Value = linha.VariacaoAno;
            ws.Range(r, 2, r, 6).Style.NumberFormat.Format = "#,##0.00";

            if (linha.EhSubtotal)
            {
                var range = ws.Range(r, 1, r, 6);
                range.Style.Font.SetBold();
                range.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F2F2F2"));
            }
            r++;

            // Detalhamento por conta analítica (indentado sob a linha do grupo)
            foreach (var conta in linha.Contas)
            {
                ws.Cell(r, 1).Value = $"{conta.Codigo} — {conta.Descricao}";
                ws.Cell(r, 1).Style.Alignment.SetIndent(2);
                ws.Cell(r, 2).Value = conta.Valor;
                ws.Cell(r, 2).Style.NumberFormat.Format = "#,##0.00";
                r++;
            }
        }

        r++;
        foreach (var (nome, valor) in new[]
        {
            ("Margem Bruta %", dre.MargemBruta),
            ("Margem Operacional %", dre.MargemOperacional),
            ("Margem Líquida %", dre.MargemLiquida),
        })
        {
            ws.Cell(r, 1).Value = nome;
            ws.Cell(r, 1).Style.Font.SetBold();
            ws.Cell(r, 2).Value = valor;
            ws.Cell(r, 2).Style.Font.SetBold();
            ws.Cell(r, 2).Style.NumberFormat.Format = "#,##0.00";
            r++;
        }

        ws.SheetView.FreezeRows(headerRow);
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);

        var arquivo = new DreExportFile(
            Conteudo: ms.ToArray(),
            ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            NomeArquivo: $"dre_{cdFilial}_{dre.Periodo.Replace('/', '-')}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");

        return Task.FromResult(arquivo);
    }

    public Task<DreExportFile> GerarPdfAsync(DreResult dre, int cdFilial, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Export DRE PDF (QuestPDF): filial {CdFilial}, período {Periodo}, {Linhas} linhas",
            cdFilial, dre.Periodo, dre.Linhas.Count);

        var pdfBytes = Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(t => t.FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().Text($"DRE — Demonstração de Resultado — {dre.Periodo}")
                    .Bold().FontSize(16);
                col.Item().Text($"Filial {cdFilial}").FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(6);
            });

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn(0.7f);
                    c.RelativeColumn(0.7f);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Linha", "Atual", "Mês Anterior", "Mesmo Mês Ano Ant.", "Var. Mês %", "Var. Ano %" })
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(4)
                            .Text(h).Bold();
                });

                foreach (var linha in dre.Linhas)
                {
                    var fundo = linha.EhSubtotal ? Colors.Grey.Lighten3 : Colors.White;

                    var descricao = table.Cell().Background(fundo).Padding(4).Text(linha.Descricao);
                    if (linha.EhSubtotal || linha.Negrito) descricao.Bold();

                    foreach (var valor in new[]
                    {
                        linha.ValorAtual, linha.ValorPeriodoAnterior, linha.ValorMesmoMesAnoAnterior,
                        linha.VariacaoMes, linha.VariacaoAno,
                    })
                    {
                        var texto = table.Cell().Background(fundo).Padding(4)
                            .AlignRight().Text(valor.ToString("N2", PtBr));
                        if (linha.EhSubtotal) texto.Bold();
                    }

                    foreach (var conta in linha.Contas)
                    {
                        table.Cell().PaddingLeft(16).Padding(3)
                            .Text($"{conta.Codigo} — {conta.Descricao}").FontColor(Colors.Grey.Darken2);
                        table.Cell().Padding(3).AlignRight()
                            .Text(conta.Valor.ToString("N2", PtBr)).FontColor(Colors.Grey.Darken2);
                        table.Cell();
                        table.Cell();
                        table.Cell();
                        table.Cell();
                    }
                }
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span($"Margem Bruta: {dre.MargemBruta.ToString("N2", PtBr)}%   ").SemiBold();
                    t.Span($"Margem Operacional: {dre.MargemOperacional.ToString("N2", PtBr)}%   ").SemiBold();
                    t.Span($"Margem Líquida: {dre.MargemLiquida.ToString("N2", PtBr)}%").SemiBold();
                });
                row.ConstantItem(60).AlignRight().Text(t =>
                {
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        })).GeneratePdf();

        var arquivo = new DreExportFile(
            Conteudo: pdfBytes,
            ContentType: "application/pdf",
            NomeArquivo: $"dre_{cdFilial}_{dre.Periodo.Replace('/', '-')}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");

        return Task.FromResult(arquivo);
    }
}
