using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.NotasFiscais.Queries;

public record ListNFeQuery(int CdFilial, int? Ano = null) : IRequest<IReadOnlyList<NFeListItemDto>>;
public record GetNFeByIdQuery(int Id) : IRequest<NFeDetailDto?>;

/// <summary>Gera o DANFE em PDF (base64) da NF-e autorizada. Retorna null se a NF-e não existir ou não tiver XML autorizado.</summary>
public record GerarDanfeQuery(int NotaFiscalId) : IRequest<string?>;

public record NFeListItemDto(
    int Id, long NumeroNF, int Serie, StatusNFe Status,
    string? ChaveAcesso, decimal ValorTotal,
    string NomeDestinatario, DateTime CriadoEm, DateTime? DataAutorizacao);

public record NFeDetailDto(
    int Id, int CdFilial, ModeloNFe Modelo, int Serie, long NumeroNF,
    StatusNFe Status, AmbienteNFe Ambiente,
    string? ChaveAcesso, string? ProtocoloAutorizacao,
    decimal ValorTotal, string NomeDestinatario, string CpfCnpjDestinatario,
    string? MotivoRejeicao, string? JustificativaCancelamento,
    DateTime CriadoEm, DateTime? DataAutorizacao, DateTime? DataCancelamento);

// Handlers
public class ListNFeQueryHandler(INFeRepository repo) : IRequestHandler<ListNFeQuery, IReadOnlyList<NFeListItemDto>>
{
    public async Task<IReadOnlyList<NFeListItemDto>> Handle(ListNFeQuery q, CancellationToken ct)
    {
        var list = await repo.GetByCdFilialAsync(q.CdFilial, q.Ano, ct);
        return list.Select(n => new NFeListItemDto(n.Id, n.NumeroNF, n.Serie, n.Status, n.ChaveAcesso, n.ValorTotal, n.NomeDestinatario, n.CriadoEm, n.DataAutorizacao)).ToList();
    }
}

public class GetNFeByIdQueryHandler(INFeRepository repo) : IRequestHandler<GetNFeByIdQuery, NFeDetailDto?>
{
    public async Task<NFeDetailDto?> Handle(GetNFeByIdQuery q, CancellationToken ct)
    {
        var n = await repo.GetByIdAsync(q.Id, ct);
        if (n is null) return null;
        return new NFeDetailDto(n.Id, n.CdFilial, n.Modelo, n.Serie, n.NumeroNF, n.Status, n.Ambiente, n.ChaveAcesso, n.ProtocoloAutorizacao, n.ValorTotal, n.NomeDestinatario, n.CpfCnpjDestinatario, n.MotivoRejeicao, n.JustificativaCancelamento, n.CriadoEm, n.DataAutorizacao, n.DataCancelamento);
    }
}

public class GerarDanfeQueryHandler(INFeRepository repo, INFeService nfeService) : IRequestHandler<GerarDanfeQuery, string?>
{
    public async Task<string?> Handle(GerarDanfeQuery q, CancellationToken ct)
    {
        var nfe = await repo.GetByIdAsync(q.NotaFiscalId, ct);
        if (nfe?.XmlAutorizado is null) return null;
        return await nfeService.GerarDanfePdfBase64Async(nfe.XmlAutorizado, ct);
    }
}
