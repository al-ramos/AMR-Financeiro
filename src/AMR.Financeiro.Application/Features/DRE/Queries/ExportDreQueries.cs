using MediatR;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.DRE.Queries;

/// <summary>Exporta a DRE do período em Excel (Card 23.4).</summary>
public record ExportDreExcelQuery(int CdFilial, int Ano, int Mes) : IRequest<DreExportFile>;

/// <summary>Exporta a DRE do período em PDF (Card 23.4).</summary>
public record ExportDrePdfQuery(int CdFilial, int Ano, int Mes) : IRequest<DreExportFile>;

public class ExportDreExcelHandler(IDreService dreService, IDreExportService exportService)
    : IRequestHandler<ExportDreExcelQuery, DreExportFile>
{
    public async Task<DreExportFile> Handle(ExportDreExcelQuery q, CancellationToken ct)
    {
        var dre = await dreService.CalcularAsync(q.CdFilial, q.Ano, q.Mes, ct);
        return await exportService.GerarExcelAsync(dre, q.CdFilial, ct);
    }
}

public class ExportDrePdfHandler(IDreService dreService, IDreExportService exportService)
    : IRequestHandler<ExportDrePdfQuery, DreExportFile>
{
    public async Task<DreExportFile> Handle(ExportDrePdfQuery q, CancellationToken ct)
    {
        var dre = await dreService.CalcularAsync(q.CdFilial, q.Ano, q.Mes, ct);
        return await exportService.GerarPdfAsync(dre, q.CdFilial, ct);
    }
}
