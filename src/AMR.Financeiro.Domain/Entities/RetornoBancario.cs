using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

public class RetornoBancario
{
    public int Id { get; private set; }
    public int CdFilial { get; private set; }
    public BancoBoleto Banco { get; private set; }
    public string ArquivoNome { get; private set; } = string.Empty;
    public string ArquivoConteudo { get; private set; } = string.Empty;
    public int TotalRegistros { get; private set; }
    public int TotalLiquidados { get; private set; }
    public decimal ValorLiquidado { get; private set; }
    public DateTime ProcessadoEm { get; private set; } = DateTime.UtcNow;

    protected RetornoBancario() { }

    public RetornoBancario(
        int cdFilial,
        BancoBoleto banco,
        string arquivoNome,
        string arquivoConteudo,
        int totalRegistros,
        int totalLiquidados,
        decimal valorLiquidado)
    {
        CdFilial = cdFilial;
        Banco = banco;
        ArquivoNome = arquivoNome;
        ArquivoConteudo = arquivoConteudo;
        TotalRegistros = totalRegistros;
        TotalLiquidados = totalLiquidados;
        ValorLiquidado = valorLiquidado;
    }
}
