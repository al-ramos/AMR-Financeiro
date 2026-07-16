using MediatR;
using AMR.Financeiro.Application.Features.Parcelamentos.Dtos;
using AMR.Financeiro.Application.Features.Parcelamentos.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Parcelamentos.Handlers;

public class GetParcelamentosHandler(IParcelamentoRepository repo)
    : IRequestHandler<GetParcelamentosQuery, List<ParcelamentoDto>>
{
    public async Task<List<ParcelamentoDto>> Handle(GetParcelamentosQuery req, CancellationToken ct)
    {
        var lista = await repo.ListarAsync(ct);
        return lista.Select(ToDto).ToList();
    }

    internal static ParcelamentoDto ToDto(Parcelamento p) => new(
        p.Id,
        p.Descricao,
        p.ValorTotal,
        p.NumeroParcelas,
        p.TipoVinculo,
        p.VinculoId,
        p.CreatedAt,
        p.Parcelas.Select(x => new ParcelaDto(
            x.Id, x.NumeroParcela, x.ValorParcela,
            x.DataVencimento, x.DataPagamento, x.Status, x.ContaBancariaId
        )).OrderBy(x => x.NumeroParcela).ToList(),
        p.Parcelas.Count(x => x.Status == StatusParcela.Pago),
        p.Parcelas.Count(x => x.Status is StatusParcela.Pendente or StatusParcela.Vencido)
    );
}

public class GetParcelamentoByIdHandler(IParcelamentoRepository repo)
    : IRequestHandler<GetParcelamentoByIdQuery, ParcelamentoDto?>
{
    public async Task<ParcelamentoDto?> Handle(GetParcelamentoByIdQuery req, CancellationToken ct)
    {
        var p = await repo.ObterPorIdAsync(req.Id, ct);
        return p is null ? null : GetParcelamentosHandler.ToDto(p);
    }
}
