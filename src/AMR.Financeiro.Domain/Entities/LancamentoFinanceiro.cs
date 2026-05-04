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

    /// <summary>Atalho para lançamento de pagamento de CP.</summary>
    public static LancamentoFinanceiro DePagamento(int cdFilial, int planoContasId, decimal valor, DateOnly data, int contaPagarId, string historico)
        => new(cdFilial, planoContasId, TipoLancamento.Debito, OrigemLancamento.ContaPagar, valor, data, historico, contaPagarId);

    /// <summary>Atalho para lançamento de recebimento de CR.</summary>
    public static LancamentoFinanceiro DeRecebimento(int cdFilial, int planoContasId, decimal valor, DateOnly data, int contaReceberId, string historico)
        => new(cdFilial, planoContasId, TipoLancamento.Credito, OrigemLancamento.ContaReceber, valor, data, historico, contaReceberId);
}
