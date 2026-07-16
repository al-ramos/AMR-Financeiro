using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.ContasBancarias.Dtos;

public record ContaBancariaDto(
    int Id,
    string Nome,
    string Banco,
    string Agencia,
    string Conta,
    TipoContaBancaria TipoConta,
    bool Ativa,
    decimal SaldoInicial,
    DateTime DataSaldoInicial,
    decimal SaldoAtual
);

public record ExtratoItemDto(
    int Id,
    DateOnly DataLancamento,
    string Historico,
    string Tipo,
    decimal Valor,
    string PlanoContas
);
