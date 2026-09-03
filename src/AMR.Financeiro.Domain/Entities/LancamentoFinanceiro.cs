using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class LancamentoFinanceiro
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }

    public int PlanoContasId { get; private set; }
    public PlanoContas PlanoContas { get; private set; } = null!;

    public TipoLancamento Tipo { get; private set; }
    public OrigemLancamento Origem { get; private set; }

    public decimal Valor { get; private set; }
    public DateOnly DataLancamento { get; private set; }
    public string Historico { get; private set; } = string.Empty;

    /// <summary>Referência ao documento de origem (ContaPagar.Id ou ContaReceber.Id)</summary>
    public int? DocumentoOrigemId { get; private set; }

    /// <summary>Conta bancária por onde o valor transitou — base do saldo/extrato multi-banco (Card 23.6).</summary>
    public int? ContaBancariaId { get; private set; }

    /// <summary>Centro de custo ao qual o lançamento é atribuído — base do realizado por CC (Card 23.5).</summary>
    public int? CentroCustoId { get; private set; }

    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected LancamentoFinanceiro() { }

    public LancamentoFinanceiro(
        int cdFilial,
        int planoContasId,
        TipoLancamento tipo,
        OrigemLancamento origem,
        decimal valor,
        DateOnly dataLancamento,
        string historico,
        int? documentoOrigemId = null)
    {
        CdFilial = cdFilial;
        PlanoContasId = planoContasId;
        Tipo = tipo;
        Origem = origem;
        Valor = valor;
        DataLancamento = dataLancamento;
        Historico = historico;
        DocumentoOrigemId = documentoOrigemId;
    }

    /// <summary>Vincula o lançamento a uma conta bancária (passa a compor saldo e extrato da conta).</summary>
    public void VincularContaBancaria(int contaBancariaId)
    {
        if (contaBancariaId <= 0)
            throw new ArgumentException("ContaBancariaId inválido.", nameof(contaBancariaId));
        ContaBancariaId = contaBancariaId;
    }

    /// <summary>Vincula o lançamento a um centro de custo (passa a compor o realizado do CC — Card 23.5).</summary>
    public void VincularCentroCusto(int centroCustoId)
    {
        if (centroCustoId <= 0)
            throw new ArgumentException("CentroCustoId inválido.", nameof(centroCustoId));
        CentroCustoId = centroCustoId;
    }

    /// <summary>Atalho para lançamento de pagamento de CP.</summary>
    public static LancamentoFinanceiro DePagamento(int cdFilial, int planoContasId, decimal valor, DateOnly data, int contaPagarId, string historico)
        => new(cdFilial, planoContasId, TipoLancamento.Debito, OrigemLancamento.ContaPagar, valor, data, historico, contaPagarId);

    /// <summary>Atalho para lançamento de recebimento de CR.</summary>
    public static LancamentoFinanceiro DeRecebimento(int cdFilial, int planoContasId, decimal valor, DateOnly data, int contaReceberId, string historico)
        => new(cdFilial, planoContasId, TipoLancamento.Credito, OrigemLancamento.ContaReceber, valor, data, historico, contaReceberId);
}
