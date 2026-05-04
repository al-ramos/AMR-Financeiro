namespace AMR.Financeiro.Application.Features.Lancamentos.Dtos;

public record LancamentoFinanceiroDto(
    int Id,
    int CdFilial,
    int PlanoContasId,
    string PlanoContasCodigo,
    string PlanoContasDescricao,
    string Tipo,
    string Origem,
    decimal Valor,
    DateOnly DataLancamento,
    string Historico,
    int? DocumentoOrigemId,
    DateTime CriadoEm
);
