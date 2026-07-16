using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>Parcela individual de um Parcelamento (Card 23.6).</summary>
public class Parcela
{
    public int Id { get; private set; }
    public int ParcelamentoId { get; private set; }
    public int NumeroParcela { get; private set; }
    public decimal ValorParcela { get; private set; }
    public DateTime DataVencimento { get; private set; }
    public DateTime? DataPagamento { get; private set; }
    public StatusParcela Status { get; private set; } = StatusParcela.Pendente;
    public int? ContaBancariaId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    protected Parcela() { }

    internal Parcela(int numeroParcela, decimal valorParcela, DateTime dataVencimento)
    {
        NumeroParcela = numeroParcela;
        ValorParcela = valorParcela;
        DataVencimento = dataVencimento;
    }

    public void MarcarPaga(DateTime dataPagamento, int? contaBancariaId)
    {
        if (Status == StatusParcela.Pago)
            throw new InvalidOperationException("Parcela já está paga.");
        if (Status == StatusParcela.Cancelado)
            throw new InvalidOperationException("Parcela cancelada não pode ser paga.");

        DataPagamento = dataPagamento;
        ContaBancariaId = contaBancariaId;
        Status = StatusParcela.Pago;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusParcela.Pago)
            throw new InvalidOperationException("Parcela paga não pode ser cancelada.");

        Status = StatusParcela.Cancelado;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Está em aberto (pendente ou vencida) — entra no aging e na projeção de fluxo.</summary>
    public bool EstaEmAberto => Status is StatusParcela.Pendente or StatusParcela.Vencido;
}
