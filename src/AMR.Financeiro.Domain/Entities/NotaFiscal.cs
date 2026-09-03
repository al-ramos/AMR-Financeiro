using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class NotaFiscal
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public ModeloNFe Modelo { get; private set; }
    public int Serie { get; private set; }
    public long NumeroNF { get; private set; }
    public StatusNFe Status { get; private set; } = StatusNFe.Digitada;
    public AmbienteNFe Ambiente { get; private set; }

    /// <summary>Chave de acesso da NF-e (44 dígitos).</summary>
    public string? ChaveAcesso { get; private set; }
    public string? ProtocoloAutorizacao { get; private set; }

    public string? XmlAssinado { get; private set; }
    public string? XmlAutorizado { get; private set; }

    public DateTime? DataAutorizacao { get; private set; }
    public DateTime? DataCancelamento { get; private set; }

    public string? MotivoRejeicao { get; private set; }
    public string? JustificativaCancelamento { get; private set; }

    public decimal ValorTotal { get; private set; }
    public string NomeDestinatario { get; private set; } = string.Empty;
    public string CpfCnpjDestinatario { get; private set; } = string.Empty;

    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected NotaFiscal() { }

    public NotaFiscal(
        int cdFilial,
        ModeloNFe modelo,
        int serie,
        long numeroNF,
        AmbienteNFe ambiente,
        decimal valorTotal,
        string nomeDestinatario,
        string cpfCnpjDestinatario)
    {
        CdFilial = cdFilial;
        Modelo = modelo;
        Serie = serie;
        NumeroNF = numeroNF;
        Ambiente = ambiente;
        ValorTotal = valorTotal;
        NomeDestinatario = nomeDestinatario;
        CpfCnpjDestinatario = cpfCnpjDestinatario;
        Status = StatusNFe.Digitada;
    }

    /// <summary>Registra o XML assinado antes do envio à SEFAZ e marca como em processamento.</summary>
    public void IniciarProcessamento(string? xmlAssinado = null)
    {
        XmlAssinado = xmlAssinado ?? XmlAssinado;
        Status = StatusNFe.Processando;
    }

    /// <summary>Autorização de uso concedida pela SEFAZ (cStat 100).</summary>
    public void Autorizar(string chaveAcesso, string protocolo, string xmlAutorizado, DateTime dataAutorizacao)
    {
        if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
            throw new ArgumentException("Chave de acesso deve conter 44 dígitos.", nameof(chaveAcesso));

        ChaveAcesso = chaveAcesso;
        ProtocoloAutorizacao = protocolo;
        XmlAutorizado = xmlAutorizado;
        DataAutorizacao = dataAutorizacao;
        MotivoRejeicao = null;
        Status = StatusNFe.Autorizada;
    }

    /// <summary>Rejeição pela SEFAZ — guarda o motivo retornado.</summary>
    public void Rejeitar(string motivo)
    {
        MotivoRejeicao = motivo;
        Status = StatusNFe.Rejeitada;
    }

    /// <summary>Cancelamento — permitido apenas para NF-e já autorizada.</summary>
    public void Cancelar(string justificativa, DateTime dataCancelamento)
    {
        if (Status != StatusNFe.Autorizada)
            throw new InvalidOperationException("Apenas NF-e autorizada pode ser cancelada.");

        JustificativaCancelamento = justificativa;
        DataCancelamento = dataCancelamento;
        Status = StatusNFe.Cancelada;
    }

    /// <summary>Inutilização de numeração não utilizada.</summary>
    public void Inutilizar() => Status = StatusNFe.Inutilizada;
}
