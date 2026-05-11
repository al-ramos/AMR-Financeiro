namespace AMR.Financeiro.Application.Features.ContasReceber.Dtos;

public record ContaReceberDto(
    int Id,
    int CdFilial,
    string Descricao,
    decimal Valor,
    DateOnly Vencimento,
    DateOnly? DataRecebimento,
    decimal? ValorRecebido,
    string? DocumentoOrigem,
    string Status
);
