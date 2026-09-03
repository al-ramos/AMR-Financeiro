using MediatR;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.Boletos.Commands;

public record GerarBoletoCommand(
    int CdFilial,
    int ContaReceberId,
    BancoBoleto Banco,
    DateOnly Vencimento,
    string Instrucao1,
    string Instrucao2) : IRequest<GerarBoletoResult>;

public record GerarBoletoResult(
    int BoletoId,
    string LinhaDigitavel,
    string CodigoBarras,
    string PdfBase64);
