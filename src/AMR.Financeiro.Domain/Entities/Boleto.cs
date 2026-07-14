using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class Boleto
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public int ContaReceberId { get; private set; }
    public BancoBoleto Banco { get; private set; }
    public string NossoNumero { get; private set; } = string.Empty;
    public string LinhaDigitavel { get; private set; } = string.Empty;
    public string CodigoBarras { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly Vencimento { get; private set; }
    public DateOnly DataEmissao { get; private set; }
    public string SacadoNome { get; private set; } = string.Empty;
    public string SacadoCpfCnpj { get; private set; } = string.Empty;
    public string SacadoEndereco { get; private set; } = string.Empty;
    public string Instrucao1 { get; private set; } = string.Empty;
    public string Instrucao2 { get; private set; } = string.Empty;
    public StatusBoleto Status { get; private set; } = StatusBoleto.Gerado;
    public DateOnly? DataPagamento { get; private set; }
    public decimal? ValorPago { get; private set; }
    public string PdfBase64 { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected Boleto() { }

    public Boleto(
        int cdFilial,
        int contaReceberId,
        BancoBoleto banco,
        string nossoNumero,
        string linhaDigitavel,
        string codigoBarras,
        decimal valor,
        DateOnly vencimento,
        string sacadoNome,
        string sacadoCpfCnpj,
        string sacadoEndereco,
        string instrucao1,
        string instrucao2,
        string pdfBase64)
    {
        CdFilial = cdFilial;
        ContaReceberId = contaReceberId;
        Banco = banco;
        NossoNumero = nossoNumero;
        LinhaDigitavel = linhaDigitavel;
        CodigoBarras = codigoBarras;
        Valor = valor;
        Vencimento = vencimento;
        DataEmissao = DateOnly.FromDateTime(DateTime.UtcNow);
        SacadoNome = sacadoNome;
        SacadoCpfCnpj = sacadoCpfCnpj;
        SacadoEndereco = sacadoEndereco;
        Instrucao1 = instrucao1;
        Instrucao2 = instrucao2;
        PdfBase64 = pdfBase64;
    }

    /// <summary>Liquidação do boleto (via retorno bancário ou baixa manual).</summary>
    public void MarcarPago(DateOnly dataPagamento, decimal valorPago)
    {
        if (Status == StatusBoleto.Cancelado)
            throw new InvalidOperationException("Boleto cancelado não pode ser marcado como pago.");

        DataPagamento = dataPagamento;
        ValorPago = valorPago;
        Status = StatusBoleto.Pago;
    }

    public void Cancelar() => Status = StatusBoleto.Cancelado;

    /// <summary>Marca o boleto como registrado no banco (após envio da remessa).</summary>
    public void MarcarRegistrado() => Status = StatusBoleto.Registrado;
}
