using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Interfaces;

public record ExtratoParseResult(
    string Banco,
    string ContaCorrente,
    DateOnly DataInicio,
    DateOnly DataFim,
    decimal SaldoInicial,
    decimal SaldoFinal,
    List<MovimentacaoParseItem> Movimentacoes);

public record MovimentacaoParseItem(
    DateOnly DataLancamento,
    TipoMovimentacao Tipo,
    decimal Valor,
    string Descricao,
    string? CodigoDoc);

public interface IExtratoParser
{
    bool Suporta(string conteudo);
    ExtratoParseResult Parse(string conteudo);
}
