using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Plano de Contas gerencial hierárquico (5 níveis) com classificação para a DRE.
/// Substitui gradualmente o plano legado <see cref="PlanoContas"/> — o vínculo entre
/// os dois é feito pelo par (CdFilial, Codigo).
/// Apenas contas de nível 5 (analíticas) aceitam lançamentos.
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

    /// <summary>Somente contas analíticas (nível 5) aceitam lançamentos diretos.</summary>
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
        int ordemExibicao)
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
        AceitaLancamentos = nivel == 5;
    }

    public void Atualizar(string descricao, GrupoDRE grupoDre, int ordemExibicao)
    {
        Descricao = descricao;
        GrupoDRE = grupoDre;
        OrdemExibicao = ordemExibicao;
    }

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;
}
