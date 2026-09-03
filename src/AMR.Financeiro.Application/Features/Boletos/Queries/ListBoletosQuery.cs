using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Boletos.Queries;

public record ListBoletosQuery(int CdFilial, StatusBoleto? Status, BancoBoleto? Banco)
    : IRequest<List<BoletoListItemDto>>;

public record BoletoListItemDto(
    int Id, string NossoNumero, BancoBoleto Banco, string SacadoNome,
    decimal Valor, DateOnly Vencimento, StatusBoleto Status, string LinhaDigitavel,
    DateOnly? DataPagamento, decimal? ValorPago);

public record GetBoletoByIdQuery(int Id) : IRequest<BoletoDetailDto?>;

public record BoletoDetailDto(
    int Id, int CdFilial, int ContaReceberId, BancoBoleto Banco,
    string NossoNumero, string LinhaDigitavel, string CodigoBarras,
    decimal Valor, DateOnly Vencimento, StatusBoleto Status,
    string SacadoNome, string SacadoCpfCnpj, string SacadoEndereco,
    string Instrucao1, string Instrucao2,
    DateOnly? DataPagamento, decimal? ValorPago, DateTime CriadoEm);

// Handlers
public class ListBoletosQueryHandler(IBoletoRepository repo)
    : IRequestHandler<ListBoletosQuery, List<BoletoListItemDto>>
{
    public async Task<List<BoletoListItemDto>> Handle(ListBoletosQuery q, CancellationToken ct)
    {
        var boletos = await repo.GetByCdFilialAsync(q.CdFilial, q.Status, q.Banco, ct);
        return boletos.Select(b => new BoletoListItemDto(
            b.Id, b.NossoNumero, b.Banco, b.SacadoNome,
            b.Valor, b.Vencimento, b.Status, b.LinhaDigitavel,
            b.DataPagamento, b.ValorPago)).ToList();
    }
}

public class GetBoletoByIdQueryHandler(IBoletoRepository repo)
    : IRequestHandler<GetBoletoByIdQuery, BoletoDetailDto?>
{
    public async Task<BoletoDetailDto?> Handle(GetBoletoByIdQuery q, CancellationToken ct)
    {
        var b = await repo.GetByIdAsync(q.Id, ct);
        if (b is null) return null;

        return new BoletoDetailDto(
            b.Id, b.CdFilial, b.ContaReceberId, b.Banco,
            b.NossoNumero, b.LinhaDigitavel, b.CodigoBarras,
            b.Valor, b.Vencimento, b.Status,
            b.SacadoNome, b.SacadoCpfCnpj, b.SacadoEndereco,
            b.Instrucao1, b.Instrucao2,
            b.DataPagamento, b.ValorPago, b.CriadoEm);
    }
}
