using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Regra de rateio de custos entre centros de custo (Card 23.5).
/// A conta de origem é referenciada pela descrição (sem FK) — a integração
/// com lançamentos reais está planejada para o Sprint 25.
/// </summary>
public class RegraRateio
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    /// <summary>Descrição da conta de origem cujos valores serão rateados.</summary>
    public string ContaOrigemDescricao { get; private set; } = string.Empty;

    public TipoBaseRateio TipoBase { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Destinos do rateio — navegação necessária para o Include no repositório.</summary>
    public ICollection<RegraRateioDestino> Destinos { get; private set; } = new List<RegraRateioDestino>();

    protected RegraRateio() { }

    public RegraRateio(int cdFilial, string nome, string contaOrigemDescricao, TipoBaseRateio tipoBase)
    {
        CdFilial = cdFilial;
        Nome = nome;
        ContaOrigemDescricao = contaOrigemDescricao;
        TipoBase = tipoBase;
    }

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;
}
