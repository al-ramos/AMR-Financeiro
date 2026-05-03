namespace RDS.Financeiro.Application.Features.ContasPagar.Dtos;

public record ContaPagarDto(
    int Id,
    int CdFilial,
    string Descricao,
    decimal Valor,
    DateOnly Vencimento,
    DateOnly? DataPagamento,
    string Status
);
