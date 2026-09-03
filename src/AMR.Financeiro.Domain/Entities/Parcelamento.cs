using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Parcelamento de um valor em N parcelas mensais (Card 23.6).
/// Pode ser avulso ou vinculado a um lançamento/boleto via TipoVinculo + VinculoId.
/// </summary>
public class Parcelamento
{
    private readonly List<Parcela> _parcelas = [];

    public int Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public int NumeroParcelas { get; private set; }
    public TipoVinculoParcelamento TipoVinculo { get; private set; }
    public int? VinculoId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public IReadOnlyCollection<Parcela> Parcelas => _parcelas.AsReadOnly();

    protected Parcelamento() { }

    public Parcelamento(
        string descricao,
        decimal valorTotal,
        int numeroParcelas,
        TipoVinculoParcelamento tipoVinculo,
        int? vinculoId)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do parcelamento é obrigatória.", nameof(descricao));
        if (valorTotal <= 0)
            throw new ArgumentException("Valor total deve ser maior que zero.", nameof(valorTotal));
        if (numeroParcelas <= 0)
            throw new ArgumentException("Número de parcelas deve ser maior que zero.", nameof(numeroParcelas));
        if (tipoVinculo != TipoVinculoParcelamento.Avulso && vinculoId is null or <= 0)
            throw new ArgumentException("VinculoId é obrigatório para parcelamentos vinculados.", nameof(vinculoId));

        Descricao = descricao.Trim();
        ValorTotal = valorTotal;
        NumeroParcelas = numeroParcelas;
        TipoVinculo = tipoVinculo;
        VinculoId = tipoVinculo == TipoVinculoParcelamento.Avulso ? null : vinculoId;
    }

    /// <summary>
    /// Gera as N parcelas com vencimentos mensais a partir do primeiro vencimento.
    /// O valor é dividido igualmente (2 casas) e a última parcela absorve os centavos restantes.
    /// </summary>
    public void GerarParcelas(DateTime primeiroVencimento)
    {
        if (_parcelas.Count > 0)
            throw new InvalidOperationException("As parcelas deste parcelamento já foram geradas.");

        var valorParcela = Math.Round(ValorTotal / NumeroParcelas, 2, MidpointRounding.ToZero);
        var valorUltima = ValorTotal - valorParcela * (NumeroParcelas - 1);

        for (var i = 1; i <= NumeroParcelas; i++)
        {
            var valor = i == NumeroParcelas ? valorUltima : valorParcela;
            _parcelas.Add(new Parcela(i, valor, primeiroVencimento.AddMonths(i - 1)));
        }
    }
}
