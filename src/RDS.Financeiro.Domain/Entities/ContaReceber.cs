using RDS.Financeiro.Domain.Enums;

namespace RDS.Financeiro.Domain.Entities;

public class ContaReceber
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly Vencimento { get; private set; }
    public DateOnly? DataRecebimento { get; private set; }
    public decimal? ValorRecebido { get; private set; }
    public StatusContaReceber Status { get; private set; } = StatusContaReceber.Aberta;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Documento de origem (NF, pedido, contrato etc.)</summary>
    public string? DocumentoOrigem { get; private set; }

    protected ContaReceber() { }

    public ContaReceber(int cdFilial, string descricao, decimal valor, DateOnly vencimento, string? documentoOrigem = null)
    {
        CdFilial = cdFilial;
        Descricao = descricao;
        Valor = valor;
        Vencimento = vencimento;
        DocumentoOrigem = documentoOrigem;
    }

    /// <summary>Baixa total — recebe o valor original.</summary>
    public void Receber(DateOnly dataRecebimento, decimal? valorRecebido = null)
    {
        DataRecebimento = dataRecebimento;
        ValorRecebido = valorRecebido ?? Valor;
        Status = StatusContaReceber.Recebida;
    }

    /// <summary>Baixa automática — usa a data de hoje e valor integral.</summary>
    public void BaixaAutomatica() => Receber(DateOnly.FromDateTime(DateTime.UtcNow));

    public void Cancelar() => Status = StatusContaReceber.Cancelada;

    public void MarcarVencida()
    {
        if (Status == StatusContaReceber.Aberta && Vencimento < DateOnly.FromDateTime(DateTime.UtcNow))
            Status = StatusContaReceber.Vencida;
    }

    public decimal Desconto() => Valor - (ValorRecebido ?? Valor);
}
