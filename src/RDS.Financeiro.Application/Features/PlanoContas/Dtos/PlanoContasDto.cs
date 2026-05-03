namespace RDS.Financeiro.Application.Features.PlanoContas.Dtos;

public record PlanoContasDto(
    int Id,
    int CdFilial,
    string Codigo,
    string Descricao,
    string Tipo,
    int? PaiId,
    string? PaiDescricao,
    int Nivel,
    bool Ativo,
    bool AceitaLancamentos
);

public record PlanoContasArvoreDto(
    int Id,
    int CdFilial,
    string Codigo,
    string Descricao,
    string Tipo,
    int? PaiId,
    int Nivel,
    bool Ativo,
    bool AceitaLancamentos,
    List<PlanoContasArvoreDto> Filhos
);
