using MediatR;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.NotasFiscais.Commands;

public record EmitirNFeCommand(
    int CdFilial,
    AmbienteNFe Ambiente,
    string NomeDestinatario,
    string CpfCnpjDestinatario,
    string EnderecoDestinatario,
    decimal ValorTotal,
    string NaturezaOperacao,
    IReadOnlyList<NFeItemDto> Itens,
    string? InformacoesAdicionais
) : IRequest<EmitirNFeResult>;

public record NFeItemDto(
    string Descricao,
    string CodigoProduto,
    decimal Quantidade,
    decimal ValorUnitario,
    string Ncm,
    string Cfop,
    string UnidadeComercial = "UN"
);

public record EmitirNFeResult(
    bool Sucesso,
    int? NotaFiscalId,
    string? ChaveAcesso,
    string? Protocolo,
    string? MensagemErro
);
