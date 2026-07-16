namespace AMR.Financeiro.Application.Features.Financeiro.Dtos;

public record AgingFaixaDto(
    string Faixa,
    int Quantidade,
    decimal ValorTotal
);

public record AgingDto(
    AgingFaixaDto AVencer,
    AgingFaixaDto De1a30,
    AgingFaixaDto De31a60,
    AgingFaixaDto De61a90,
    AgingFaixaDto Acima90,
    decimal TotalEmAberto
);

public record FluxoCaixaDiaDto(
    DateTime Data,
    decimal Entradas,
    decimal Saidas,
    decimal Saldo
);

public record FluxoCaixaDto(
    int HorizonteDias,
    List<FluxoCaixaDiaDto> Dias,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoFinal
);
