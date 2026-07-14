using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Centro de custo hierárquico (até 3 níveis) por filial (Card 23.5).
/// </summary>
public class CentroCusto
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public TipoCentroCusto Tipo { get; private set; }
    public int? PaiId { get; private set; }

    /// <summary>Nível hierárquico: 1 (grupo) a 3 (analítico).</summary>
    public int Nivel { get; private set; }

    public string ResponsavelNome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    protected CentroCusto() { }

    public CentroCusto(int cdFilial, string codigo, string descricao, TipoCentroCusto tipo,
        int? paiId, int nivel, string responsavelNome)
    {
        if (nivel is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nível deve estar entre 1 e 3.");

        CdFilial = cdFilial;
        Codigo = codigo;
        Descricao = descricao;
        Tipo = tipo;
        PaiId = paiId;
        Nivel = nivel;
        ResponsavelNome = responsavelNome;
    }

    public void Atualizar(string descricao, TipoCentroCusto tipo, string responsavelNome)
    {
        Descricao = descricao;
        Tipo = tipo;
        ResponsavelNome = responsavelNome;
    }

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;
}
