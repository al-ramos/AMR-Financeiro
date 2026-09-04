using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Regra de rateio de custos entre centros de custo (Card 23.5).
///
/// A conta de origem é referenciada por <see cref="ContaOrigemId"/>, uma FK para o
/// plano de contas. Antes era só uma descrição solta, e o serviço de rateio, sem ter
/// como encontrar a conta, distribuía um valor fixo de R$ 1.000 — que era persistido
/// e entrava na DRE e no orçamento como se fosse apuração. Ver FIN-02.
/// </summary>
public class RegraRateio
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    /// <summary>Conta do plano cujos lançamentos do mês são o valor a ratear.</summary>
    public int ContaOrigemId { get; private set; }
    public PlanoDeContas ContaOrigem { get; private set; } = null!;

    /// <summary>Rótulo da conta de origem, para exibição. O valor vem de <see cref="ContaOrigemId"/>.</summary>
    public string ContaOrigemDescricao { get; private set; } = string.Empty;

    public TipoBaseRateio TipoBase { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Destinos do rateio — navegação necessária para o Include no repositório.</summary>
    public ICollection<RegraRateioDestino> Destinos { get; private set; } = new List<RegraRateioDestino>();

    protected RegraRateio() { }

    public RegraRateio(int cdFilial, string nome, int contaOrigemId, string contaOrigemDescricao, TipoBaseRateio tipoBase)
    {
        if (contaOrigemId <= 0)
            throw new ArgumentException("A regra precisa de uma conta de origem.", nameof(contaOrigemId));

        CdFilial = cdFilial;
        Nome = nome;
        ContaOrigemId = contaOrigemId;
        ContaOrigemDescricao = contaOrigemDescricao;
        TipoBase = tipoBase;
    }

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;
}
