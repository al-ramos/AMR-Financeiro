using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Interfaces;

public record BoletoGeradoResult(
    string NossoNumero,
    string LinhaDigitavel,
    string CodigoBarras,
    string PdfBase64);

public record RemessaGeradaResult(
    string NomeArquivo,
    string CnabBase64,
    string CnabConteudo,
    int TotalBoletos,
    decimal ValorTotal);

public record RetornoProcessadoItem(
    string NossoNumero,
    DateOnly DataPagamento,
    decimal ValorPago,
    bool Sucesso,
    string? Erro);

public interface IBoletoService
{
    Task<BoletoGeradoResult> GerarAsync(
        BancoBoleto banco, int nossoNumero,
        string sacadoNome, string sacadoCpfCnpj, string sacadoEndereco,
        decimal valor, DateOnly vencimento,
        string instrucao1, string instrucao2,
        CancellationToken ct = default);

    Task<RemessaGeradaResult> GerarRemessaAsync(
        BancoBoleto banco, TipoCnab tipo,
        List<Boleto> boletos, CancellationToken ct = default);

    Task<List<RetornoProcessadoItem>> ProcessarRetornoAsync(
        string conteudoArquivo, BancoBoleto banco, CancellationToken ct = default);
}
