namespace AMR.Financeiro.Application.Interfaces;

/// <summary>Arquivo exportado da DRE (Excel ou PDF) — Card 23.4.</summary>
public record DreExportFile(byte[] Conteudo, string ContentType, string NomeArquivo);

/// <summary>
/// Exportação da DRE para Excel e PDF.
/// Implementação atual em Infrastructure (DreExportService) é um stub profissional
/// no estilo do NFeService (Card 23.1): gera arquivos reais e abríveis, com os
/// pontos de integração de ClosedXML (Excel) e QuestPDF (PDF) comentados no código.
/// </summary>
public interface IDreExportService
{
    Task<DreExportFile> GerarExcelAsync(DreResult dre, int cdFilial, CancellationToken ct = default);
    Task<DreExportFile> GerarPdfAsync(DreResult dre, int cdFilial, CancellationToken ct = default);
}
