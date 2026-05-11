namespace AMR.Financeiro.Domain.Entities;

public class ContaPagar
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly Vencimento { get; private set; }
    public DateOnly? DataPagamento { get; private set; }
    public StatusConta Status { get; private set; } = StatusConta.Aberta;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected ContaPagar() {}

    public ContaPagar(int cdFilial, string descricao, decimal valor, DateOnly vencimento)
    {
        CdFilial = cdFilial;
        Descricao = descricao;
        Valor = valor;
        Vencimento = vencimento;
    }

    public void Pagar(DateOnly dataPagamento)
    {
        DataPagamento = dataPagamento;
        Status = StatusConta.Paga;
    }

    public void Cancelar() => Status = StatusConta.Cancelada;

    public void MarcarVencida()
    {
        if (Status == StatusConta.Aberta && Vencimento < DateOnly.FromDateTime(DateTime.UtcNow))
            Status = StatusConta.Vencida;
    }
}
