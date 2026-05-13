namespace AMR.Financeiro.Domain.Events;

public class LancamentoCriadoEvent
{
    public int LancamentoId { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataCriacao { get; init; }
}
