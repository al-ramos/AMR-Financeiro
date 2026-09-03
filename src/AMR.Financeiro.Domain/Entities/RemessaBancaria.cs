using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class RemessaBancaria
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public BancoBoleto Banco { get; private set; }
    public TipoCnab TipoCnab { get; private set; }
    public string ArquivoCnab { get; private set; } = string.Empty;
    public string NomeArquivo { get; private set; } = string.Empty;
    public int TotalBoletos { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    protected RemessaBancaria() { }

    public RemessaBancaria(
        int cdFilial,
        BancoBoleto banco,
        TipoCnab tipoCnab,
        string arquivoCnab,
        string nomeArquivo,
        int totalBoletos,
        decimal valorTotal)
    {
        CdFilial = cdFilial;
        Banco = banco;
        TipoCnab = tipoCnab;
        ArquivoCnab = arquivoCnab;
        NomeArquivo = nomeArquivo;
        TotalBoletos = totalBoletos;
        ValorTotal = valorTotal;
    }
}
