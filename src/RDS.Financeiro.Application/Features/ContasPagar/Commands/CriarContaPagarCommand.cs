using MediatR;

namespace RDS.Financeiro.Application.Features.ContasPagar.Commands;

public record CriarContaPagarCommand(
    int CdFilial,
    string Descricao,
    decimal Valor,
    DateOnly Vencimento
) : IRequest<int>;

public record PagarContaCommand(int Id, DateOnly DataPagamento) : IRequest<bool>;
public record CancelarContaCommand(int Id) : IRequest<bool>;
