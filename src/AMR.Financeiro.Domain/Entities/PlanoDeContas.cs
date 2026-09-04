using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Plano de contas único do módulo: é o razão (para onde os lançamentos apontam) e,
/// ao mesmo tempo, carrega a classificação gerencial usada pela DRE.
///
/// Cobre tanto as contas patrimoniais (Ativo/Passivo, com <see cref="GrupoDRE.NaoAplicavel"/>)
/// quanto as de resultado. Antes existia um plano legado separado, e o vínculo entre os dois
/// era feito pelo par (CdFilial, Codigo) — o que permitia que um Id da tela fosse resolvido
/// contra a outra tabela e o lançamento acabasse em outra conta. Ver FIN-01.
///
/// Só contas analíticas (folhas) aceitam lançamentos; a sintética existe para agrupar.
/// </summary>
public class PlanoDeContas
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }

    /// <summary>Código hierárquico ex: 3, 3.1, 3.1.1, 3.1.1.1, 3.1.1.1.1</summary>
    public string Codigo { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;
    public TipoContaContabil Tipo { get; private set; }
    public NaturezaConta Natureza { get; private set; }

    /// <summary>Nível hierárquico: 1 (grupo) a 5 (analítica).</summary>
    public int Nivel { get; private set; }

    public int? PaiId { get; private set; }

    /// <summary>
    /// Conta analítica: aceita lançamento direto. Declarado explicitamente em vez de
    /// derivado do nível — amarrar em "nível 5" obrigava a inventar profundidade para
    /// contas que são analíticas no nível 3, como 1.1.3 Contas a Receber.
    /// </summary>
    public bool AceitaLancamentos { get; private set; }

    public GrupoDRE GrupoDRE { get; private set; }
    public int OrdemExibicao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected PlanoDeContas() { }

    public PlanoDeContas(
        int cdFilial,
        string codigo,
        string descricao,
        TipoContaContabil tipo,
        NaturezaConta natureza,
        int nivel,
        int? paiId,
        GrupoDRE grupoDre,
        int ordemExibicao,
        bool aceitaLancamentos = false)
    {
        if (nivel is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nível deve estar entre 1 e 5.");

        CdFilial = cdFilial;
        Codigo = codigo;
        Descricao = descricao;
        Tipo = tipo;
        Natureza = natureza;
        Nivel = nivel;
        PaiId = paiId;
        GrupoDRE = grupoDre;
        OrdemExibicao = ordemExibicao;
        AceitaLancamentos = aceitaLancamentos;
    }

    public void Atualizar(string descricao, GrupoDRE grupoDre, int ordemExibicao)
    {
        Descricao = descricao;
        GrupoDRE = grupoDre;
        OrdemExibicao = ordemExibicao;
    }

    /// <summary>Converte a conta em sintética ou analítica após a criação.</summary>
    public void DefinirAceitaLancamentos(bool aceita) => AceitaLancamentos = aceita;

    /// <summary>Conta patrimonial — existe no razão, mas não compõe nenhuma linha da DRE.</summary>
    public bool EhPatrimonial() => GrupoDRE == GrupoDRE.NaoAplicavel;

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;
}
