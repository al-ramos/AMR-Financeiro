using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class MovimentacaoBancaria
{
    public int Id { get; private set; }
    public int ExtratoId { get; private set; }
    public DateOnly DataLancamento { get; private set; }
    public TipoMovimentacao Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public string? CodigoDoc { get; private set; }
    public StatusConciliacao StatusConciliacao { get; private set; } = StatusConciliacao.Pendente;

    /// <summary>Vínculo com o lançamento financeiro conciliado.</summary>
    public int? LancamentoId { get; private set; }

    public DateTime? ConciliadoEm { get; private set; }
    public string? ConciliadoPor { get; private set; }

    protected MovimentacaoBancaria() { }

    public MovimentacaoBancaria(
        int extratoId,
        DateOnly dataLancamento,
        TipoMovimentacao tipo,
        decimal valor,
        string descricao,
        string? codigoDoc)
    {
        ExtratoId = extratoId;
        DataLancamento = dataLancamento;
        Tipo = tipo;
        Valor = valor;
        Descricao = descricao;
        CodigoDoc = codigoDoc;
        StatusConciliacao = StatusConciliacao.Pendente;
    }

    public void ConciliarCom(int lancamentoId, string conciliadoPor)
    {
        LancamentoId = lancamentoId;
        StatusConciliacao = StatusConciliacao.Conciliado;
        ConciliadoEm = DateTime.UtcNow;
        ConciliadoPor = conciliadoPor;
    }

    public void Ignorar(string motivo)
    {
        StatusConciliacao = StatusConciliacao.Ignorado;
        ConciliadoPor = $"Ignorado: {motivo}";
        ConciliadoEm = DateTime.UtcNow;
    }

    public void MarcarDivergente() => StatusConciliacao = StatusConciliacao.Divergente;
}
