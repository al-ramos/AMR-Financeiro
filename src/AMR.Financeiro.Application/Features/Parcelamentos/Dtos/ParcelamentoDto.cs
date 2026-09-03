using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.Parcelamentos.Dtos;

public record ParcelaDto(
    int Id,
    int NumeroParcela,
    decimal ValorParcela,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    StatusParcela Status,
    int? ContaBancariaId
);

public record ParcelamentoDto(
    int Id,
    string Descricao,
    decimal ValorTotal,
    int NumeroParcelas,
    TipoVinculoParcelamento TipoVinculo,
    int? VinculoId,
    DateTime CreatedAt,
    List<ParcelaDto> Parcelas,
    int TotalPagas,
    int TotalPendentes
);
