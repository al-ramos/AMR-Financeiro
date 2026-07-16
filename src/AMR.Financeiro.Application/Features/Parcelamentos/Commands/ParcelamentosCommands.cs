using MediatR;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.Parcelamentos.Commands;

public record CriarParcelamentoCommand(
    string Descricao,
    decimal ValorTotal,
    int NumeroParcelas,
    TipoVinculoParcelamento TipoVinculo,
    int? VinculoId,
    DateTime PrimeiroVencimento
) : IRequest<int>;

public record PagarParcelaCommand(
    int ParcelamentoId,
    int ParcelaId,
    DateTime DataPagamento,
    int? ContaBancariaId
) : IRequest<bool>;
