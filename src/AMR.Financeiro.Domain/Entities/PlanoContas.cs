using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class PlanoContas
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }

    /// <summary>Código hierárquico ex: 1, 1.1, 1.1.1</summary>
    public string Codigo { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;
    public TipoContaPlano Tipo { get; private set; }

    public int? PaiId { get; private set; }
    public PlanoContas? Pai { get; private set; }

    public ICollection<PlanoContas> Filhos { get; private set; } = new List<PlanoContas>();
    public ICollection<LancamentoFinanceiro> Lancamentos { get; private set; } = new List<LancamentoFinanceiro>();

    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected PlanoContas() { }

    public PlanoContas(int cdFilial, string codigo, string descricao, TipoContaPlano tipo, int? paiId = null)
    {
        CdFilial = cdFilial;
        Codigo = codigo;
        Descricao = descricao;
        Tipo = tipo;
        PaiId = paiId;
    }

    public void Atualizar(string descricao) => Descricao = descricao;

    public void Inativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    /// <summary>Conta analítica aceita lançamentos diretos.</summary>
    public bool AceitaLancamentos() => Tipo == TipoContaPlano.Analitica;

    /// <summary>Nível hierárquico baseado no código (ex: 1.1.2 = nível 3).</summary>
    public int Nivel() => Codigo.Count(c => c == '.') + 1;
}
