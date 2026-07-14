using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class ExtratoBancario
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public string Banco { get; private set; } = string.Empty;
    public string ContaCorrente { get; private set; } = string.Empty;
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public decimal SaldoInicial { get; private set; }
    public decimal SaldoFinal { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public FormatoExtrato Formato { get; private set; }
    public string ArquivoOriginal { get; private set; } = string.Empty;
    public DateTime ImportadoEm { get; private set; } = DateTime.UtcNow;

    protected ExtratoBancario() { }

    public ExtratoBancario(
        int cdFilial,
        string banco,
        string contaCorrente,
        DateOnly dataInicio,
        DateOnly dataFim,
        decimal saldoInicial,
        decimal saldoFinal,
        decimal totalCreditos,
        decimal totalDebitos,
        FormatoExtrato formato,
        string arquivoOriginal)
    {
        CdFilial = cdFilial;
        Banco = banco;
        ContaCorrente = contaCorrente;
        DataInicio = dataInicio;
        DataFim = dataFim;
        SaldoInicial = saldoInicial;
        SaldoFinal = saldoFinal;
        TotalCreditos = totalCreditos;
        TotalDebitos = totalDebitos;
        Formato = formato;
        ArquivoOriginal = arquivoOriginal;
        ImportadoEm = DateTime.UtcNow;
    }
}
