using MediatR;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.Lancamentos.Commands;

public record CriarLancamentoCommand(
    int CdFilial,
    int PlanoContasId,
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly DataLancamento,
    string Historico,
    int? DocumentoOrigemId = null
) : IRequest<int>;
