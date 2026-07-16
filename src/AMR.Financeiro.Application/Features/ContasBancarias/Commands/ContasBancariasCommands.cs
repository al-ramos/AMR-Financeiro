using MediatR;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.ContasBancarias.Commands;

public record CriarContaBancariaCommand(
    string Nome,
    string Banco,
    string Agencia,
    string Conta,
    TipoContaBancaria TipoConta,
    decimal SaldoInicial,
    DateTime DataSaldoInicial
) : IRequest<int>;

public record AtualizarContaBancariaCommand(
    int Id,
    string Nome,
    string Banco,
    string Agencia,
    string Conta,
    TipoContaBancaria TipoConta,
    decimal SaldoInicial,
    DateTime DataSaldoInicial
) : IRequest<bool>;

public record DesativarContaBancariaCommand(int Id) : IRequest<bool>;
