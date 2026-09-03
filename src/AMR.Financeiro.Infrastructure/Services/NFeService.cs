using System.Text;
using Microsoft.Extensions.Logging;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Stub profissional da integração NF-e (Card 23.1).
///
/// Em HOMOLOGAÇÃO simula todo o fluxo de autorização/cancelamento da SEFAZ,
/// retornando respostas realistas (chave de 44 dígitos com DV mod-11 válido,
/// protocolo e XML autorizado fake).
///
/// Em PRODUÇÃO lança NotImplementedException — a integração real será feita
/// com o pacote DFe.NET (Zeus) quando o certificado digital A1 da filial
/// estiver configurado. Os comentários em cada etapa indicam onde o DFe.NET
/// entra no fluxo real.
/// </summary>
public class NFeService(ILogger<NFeService> logger) : INFeService
{
    // Na integração real estes valores virão do cadastro da filial (appsettings/DB):
    // UF do emitente, CNPJ, certificado digital e URLs dos webservices da SEFAZ.
    private const string CodigoUfEmitente = "35";              // 35 = São Paulo
    private const string CnpjEmitenteHomologacao = "99999999000191";

    public async Task<NFeEmissaoResult> EmitirAsync(NFeEmissaoRequest request, long numeroNF, int serie, CancellationToken ct = default)
    {
        if (request.Ambiente == AmbienteNFe.Producao)
            throw new NotImplementedException(
                "Emissão de NF-e em ambiente de PRODUÇÃO ainda não está habilitada. " +
                "Requer integração real com a SEFAZ via DFe.NET e certificado digital A1 configurado para a filial.");

        // ------------------------------------------------------------------
        // FLUXO REAL COM DFe.NET (referência para a implementação definitiva):
        //
        // 1. MONTAR o objeto NFe.Classes.NFe a partir do request:
        //    ide (cUF, natOp, mod=55, serie, nNF, tpAmb), emit (dados da filial),
        //    dest (Nome/CpfCnpj/Endereço), det[] (itens com NCM/CFOP/valores),
        //    total (ICMSTot), transp, pag, infAdic.
        //
        // 2. ASSINAR o XML com o certificado A1 da filial:
        //    var servicoNFe = new ServicosNFe(configuracaoServico);
        //    (a ConfiguracaoServico carrega o X509Certificate2 e o tpAmb).
        //
        // 3. ENVIAR o lote de forma síncrona (NFeAutorizacao4):
        //    var retorno = servicoNFe.NFeAutorizacao(idLote, IndicadorSincronizacao.Sincrono, new List<NFe.Classes.NFe> { nfe });
        //
        // 4. TRATAR o retorno: cStat 100 = "Autorizado o uso da NF-e" →
        //    extrair chNFe, nProt e o XML do nfeProc (procNFe); qualquer outro
        //    cStat (ex.: 204 duplicidade, 539 chave divergente) = rejeição.
        // ------------------------------------------------------------------

        logger.LogInformation(
            "SIMULAÇÃO NF-e (homologação): emitindo NF-e nº {NumeroNF} série {Serie} para filial {CdFilial}, destinatário {Destinatario}, valor {ValorTotal:C}",
            numeroNF, serie, request.CdFilial, request.NomeDestinatario, request.ValorTotal);

        // Simula a latência do webservice da SEFAZ (envio + consulta de recibo).
        await Task.Delay(150, ct);

        var chaveAcesso = GerarChaveAcessoFake(numeroNF, serie);
        var protocolo = $"135{DateTime.UtcNow:yyyyMMddHHmmss}";
        var xmlAutorizado = GerarXmlAutorizadoFake(chaveAcesso, protocolo, request, numeroNF, serie);

        logger.LogInformation(
            "SIMULAÇÃO NF-e (homologação): autorizada. Chave {ChaveAcesso}, protocolo {Protocolo}",
            chaveAcesso, protocolo);

        return new NFeEmissaoResult(
            Sucesso: true,
            ChaveAcesso: chaveAcesso,
            Protocolo: protocolo,
            XmlAutorizado: xmlAutorizado,
            MensagemErro: null,
            CodigoRetorno: "100"); // 100 = Autorizado o uso da NF-e
    }

    public async Task<NfeCancelamentoResult> CancelarAsync(string chaveAcesso, string justificativa, AmbienteNFe ambiente, CancellationToken ct = default)
    {
        if (ambiente == AmbienteNFe.Producao)
            throw new NotImplementedException(
                "Cancelamento de NF-e em ambiente de PRODUÇÃO ainda não está habilitado. " +
                "Requer integração real com a SEFAZ via DFe.NET (evento 110111) e certificado digital A1.");

        // Regra da SEFAZ: justificativa entre 15 e 255 caracteres.
        if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Trim().Length < 15)
            return new NfeCancelamentoResult(false, "Justificativa de cancelamento deve ter no mínimo 15 caracteres (regra SEFAZ).");

        // ------------------------------------------------------------------
        // FLUXO REAL COM DFe.NET:
        //
        // 1. Montar o evento de cancelamento (tpEvento 110111) com chNFe,
        //    nProt da autorização e xJust.
        // 2. Assinar e enviar via RecepcaoEvento4:
        //    var retorno = servicoNFe.RecepcaoEventoCancelamento(idLote, sequenciaEvento, protocolo, chaveAcesso, justificativa, cnpj);
        // 3. cStat 135 = "Evento registrado e vinculado a NF-e" → sucesso.
        // ------------------------------------------------------------------

        logger.LogInformation(
            "SIMULAÇÃO NF-e (homologação): cancelando NF-e chave {ChaveAcesso}. Justificativa: {Justificativa}",
            chaveAcesso, justificativa);

        await Task.Delay(100, ct);

        return new NfeCancelamentoResult(true, null);
    }

    public Task<string> GerarDanfePdfBase64Async(string xmlAutorizado, CancellationToken ct = default)
    {
        // ------------------------------------------------------------------
        // FLUXO REAL: usar um renderizador de DANFE a partir do XML autorizado,
        // ex.: Zeus.DanfeSharp / NFe.Danfe.Base (do próprio ecossistema DFe.NET),
        // que recebe o nfeProc e devolve o PDF do DANFE (retrato, modelo 55).
        // ------------------------------------------------------------------

        // Stub: gera um PDF mínimo válido com aviso de homologação.
        var pdf = new StringBuilder();
        pdf.AppendLine("%PDF-1.4");
        pdf.AppendLine("1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj");
        pdf.AppendLine("2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj");
        pdf.AppendLine("3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj");
        const string texto = "BT /F1 14 Tf 50 790 Td (DANFE - DOCUMENTO AUXILIAR DA NF-e) Tj 0 -24 Td (EMITIDO EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL) Tj ET";
        pdf.AppendLine($"4 0 obj << /Length {texto.Length} >> stream");
        pdf.AppendLine(texto);
        pdf.AppendLine("endstream endobj");
        pdf.AppendLine("5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj");
        pdf.AppendLine("trailer << /Root 1 0 R >>");
        pdf.AppendLine("%%EOF");

        var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(pdf.ToString()));
        return Task.FromResult(base64);
    }

    /// <summary>
    /// Gera uma chave de acesso fake de 44 dígitos seguindo o layout oficial:
    /// cUF(2) + AAMM(4) + CNPJ(14) + mod(2) + série(3) + nNF(9) + tpEmis(1) + cNF(8) + cDV(1).
    /// </summary>
    private static string GerarChaveAcessoFake(long numeroNF, int serie)
    {
        var agora = DateTime.UtcNow;
        var semDv =
            CodigoUfEmitente +
            agora.ToString("yyMM") +
            CnpjEmitenteHomologacao +
            ((int)ModeloNFe.NFe).ToString("D2") +
            serie.ToString("D3") +
            numeroNF.ToString("D9") +
            "1" + // tpEmis = 1 (emissão normal)
            Random.Shared.Next(0, 99999999).ToString("D8");

        return semDv + CalcularDigitoVerificador(semDv);
    }

    /// <summary>Dígito verificador módulo 11 da chave de acesso (43 dígitos → 1 dígito).</summary>
    private static int CalcularDigitoVerificador(string chave43)
    {
        int peso = 2, soma = 0;
        for (var i = chave43.Length - 1; i >= 0; i--)
        {
            soma += (chave43[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        var resto = soma % 11;
        return resto is 0 or 1 ? 0 : 11 - resto;
    }

    private static string GerarXmlAutorizadoFake(string chaveAcesso, string protocolo, NFeEmissaoRequest request, long numeroNF, int serie)
    {
        // Estrutura simplificada do nfeProc (na integração real, é o XML devolvido pela SEFAZ).
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<nfeProc versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
        sb.Append($"<NFe><infNFe Id=\"NFe{chaveAcesso}\" versao=\"4.00\">");
        sb.Append($"<ide><cUF>{CodigoUfEmitente}</cUF><mod>55</mod><serie>{serie}</serie><nNF>{numeroNF}</nNF>");
        sb.Append($"<natOp>{request.NaturezaOperacao}</natOp><tpAmb>{(int)request.Ambiente}</tpAmb></ide>");
        sb.Append($"<emit><CNPJ>{CnpjEmitenteHomologacao}</CNPJ></emit>");
        sb.Append($"<dest><xNome>{request.NomeDestinatario}</xNome></dest>");
        sb.Append($"<total><ICMSTot><vNF>{request.ValorTotal:0.00}</vNF></ICMSTot></total>");
        sb.Append("</infNFe></NFe>");
        sb.Append("<protNFe versao=\"4.00\"><infProt>");
        sb.Append($"<tpAmb>{(int)request.Ambiente}</tpAmb><chNFe>{chaveAcesso}</chNFe>");
        sb.Append($"<dhRecbto>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:sszzz}</dhRecbto><nProt>{protocolo}</nProt>");
        sb.Append("<cStat>100</cStat><xMotivo>Autorizado o uso da NF-e (SIMULACAO HOMOLOGACAO)</xMotivo>");
        sb.Append("</infProt></protNFe></nfeProc>");
        return sb.ToString();
    }
}
