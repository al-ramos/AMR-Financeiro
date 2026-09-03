using Moq;
using AMR.Financeiro.Application.Features.DRE.Queries;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Tests.Features.DRE;

public class ExportDreHandlerTests
{
    private readonly Mock<IDreService> _dreServiceMock = new();
    private readonly Mock<IDreExportService> _exportServiceMock = new();

    private static DreResult DreVazia() => new("07/2026", new List<LinhasDRE>(), 0, 0, 0);

    [Fact]
    public async Task HandleExcel_CalculaDreEDelegaParaExportacao()
    {
        var dre = DreVazia();
        var arquivo = new DreExportFile(new byte[] { 1, 2, 3 }, "application/vnd.ms-excel", "dre.xls");

        _dreServiceMock.Setup(s => s.CalcularAsync(1, 2026, 7, default)).ReturnsAsync(dre);
        _exportServiceMock.Setup(s => s.GerarExcelAsync(dre, 1, default)).ReturnsAsync(arquivo);

        var handler = new ExportDreExcelHandler(_dreServiceMock.Object, _exportServiceMock.Object);
        var result = await handler.Handle(new ExportDreExcelQuery(1, 2026, 7), default);

        Assert.Same(arquivo, result);
        _dreServiceMock.Verify(s => s.CalcularAsync(1, 2026, 7, default), Times.Once);
        _exportServiceMock.Verify(s => s.GerarExcelAsync(dre, 1, default), Times.Once);
        _exportServiceMock.Verify(s => s.GerarPdfAsync(It.IsAny<DreResult>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandlePdf_CalculaDreEDelegaParaExportacao()
    {
        var dre = DreVazia();
        var arquivo = new DreExportFile(new byte[] { 4, 5, 6 }, "application/pdf", "dre.pdf");

        _dreServiceMock.Setup(s => s.CalcularAsync(2, 2026, 6, default)).ReturnsAsync(dre);
        _exportServiceMock.Setup(s => s.GerarPdfAsync(dre, 2, default)).ReturnsAsync(arquivo);

        var handler = new ExportDrePdfHandler(_dreServiceMock.Object, _exportServiceMock.Object);
        var result = await handler.Handle(new ExportDrePdfQuery(2, 2026, 6), default);

        Assert.Same(arquivo, result);
        _dreServiceMock.Verify(s => s.CalcularAsync(2, 2026, 6, default), Times.Once);
        _exportServiceMock.Verify(s => s.GerarPdfAsync(dre, 2, default), Times.Once);
        _exportServiceMock.Verify(s => s.GerarExcelAsync(It.IsAny<DreResult>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleExcel_ArquivoRetornadoTemConteudoEContentType()
    {
        var dre = DreVazia();
        _dreServiceMock.Setup(s => s.CalcularAsync(1, 2026, 7, default)).ReturnsAsync(dre);
        _exportServiceMock.Setup(s => s.GerarExcelAsync(dre, 1, default))
            .ReturnsAsync(new DreExportFile(new byte[] { 9 }, "application/vnd.ms-excel", "dre_1_07-2026.xls"));

        var handler = new ExportDreExcelHandler(_dreServiceMock.Object, _exportServiceMock.Object);
        var result = await handler.Handle(new ExportDreExcelQuery(1, 2026, 7), default);

        Assert.NotEmpty(result.Conteudo);
        Assert.Equal("application/vnd.ms-excel", result.ContentType);
        Assert.EndsWith(".xls", result.NomeArquivo);
    }
}
