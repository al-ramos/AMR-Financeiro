using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Interfaces;

public record NFeEmissaoRequest(
    int CdFilial,
    AmbienteNFe Ambiente,
    string NomeDestinatario,
    string CpfCnpjDestinatario,
    string EnderecoDestinatario,
    decimal ValorTotal,
    string NaturezaOperacao,
    IReadOnlyList<NFeItemRequest> Itens,
    string? InformacoesAdicionais = null
);

public record NFeItemRequest(
    string Descricao,
    string CodigoProduto,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    string Ncm,
    string Cfop,
    string UnidadeComercial = "UN"
);

public record NFeEmissaoResult(
    bool Sucesso,
    string? ChaveAcesso,
    string? Protocolo,
    string? XmlAutorizado,
    string? MensagemErro,
    string? CodigoRetorno
);

public record NfeCancelamentoResult(
    bool Sucesso,
    string? MensagemErro
);

public interface INFeService
{
    Task<NFeEmissaoResult> EmitirAsync(NFeEmissaoRequest request, long numeroNF, int serie, CancellationToken ct = default);
    Task<NfeCancelamentoResult> CancelarAsync(string chaveAcesso, string justificativa, AmbienteNFe ambiente, CancellationToken ct = default);
    Task<string> GerarDanfePdfBase64Async(string xmlAutorizado, CancellationToken ct = default);
}
