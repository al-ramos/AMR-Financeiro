using MediatR;

namespace AMR.Financeiro.Application.Features.ContasReceber.Commands;

public record CriarContaReceberCommand(
    int CdFilial,
    string Descricao,
    decimal Valor,
    DateOnly Vencimento,
    string? DocumentoOrigem = null
) : IRequest<int>;

public record ReceberContaCommand(
    int Id,
    DateOnly DataRecebimento,
    decimal? ValorRecebido = null
) : IRequest<bool>;

public record CancelarContaReceberCommand(int Id) : IRequest<bool>;
