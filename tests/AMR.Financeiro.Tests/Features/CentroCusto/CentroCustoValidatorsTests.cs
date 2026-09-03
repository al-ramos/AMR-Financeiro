using AMR.Financeiro.Application.Features.CentroCusto.Commands;
using AMR.Financeiro.Application.Features.CentroCusto.Validators;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Tests.Features.CentroCusto;

public class CentroCustoValidatorsTests
{
    // ---------- CriarCentroCustoCommandValidator ----------

    private readonly CriarCentroCustoCommandValidator _criarCcValidator = new();

    [Theory]
    [InlineData("1", 1, true)]
    [InlineData("1.2", 2, true)]
    [InlineData("1.2.3", 3, true)]
    [InlineData("1.2.3.4", 4, false)] // 4 níveis não suportados
    [InlineData("abc", 1, false)]     // código não numérico
    [InlineData("1.2", 1, false)]     // nível não corresponde à profundidade
    public void CriarCentroCusto_ValidaFormatoHierarquicoDoCodigo(string codigo, int nivel, bool esperado)
    {
        var cmd = new CriarCentroCustoCommand(1, codigo, "Descrição", TipoCentroCusto.Produtivo,
            nivel == 1 ? null : 7, nivel, "Responsável");

        Assert.Equal(esperado, _criarCcValidator.Validate(cmd).IsValid);
    }

    [Fact]
    public void CriarCentroCusto_Nivel1ComPai_EhInvalido()
    {
        var cmd = new CriarCentroCustoCommand(1, "1", "Fábrica", TipoCentroCusto.Produtivo,
            7, 1, "Alessandro");

        var result = _criarCcValidator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("não pode ter pai"));
    }

    [Fact]
    public void CriarCentroCusto_Nivel2SemPai_EhInvalido()
    {
        var cmd = new CriarCentroCustoCommand(1, "1.2", "Usinagem", TipoCentroCusto.Produtivo,
            null, 2, "Alessandro");

        var result = _criarCcValidator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("deve informar o centro de custo pai"));
    }

    [Fact]
    public void CriarCentroCusto_ResponsavelVazio_EhInvalido()
    {
        var cmd = new CriarCentroCustoCommand(1, "1", "Fábrica", TipoCentroCusto.Produtivo,
            null, 1, "");

        Assert.False(_criarCcValidator.Validate(cmd).IsValid);
    }

    // ---------- CriarRegraRateioCommandValidator ----------

    private readonly CriarRegraRateioCommandValidator _criarRegraValidator = new();

    [Fact]
    public void CriarRegraRateio_PercentualFixoSomando100_EhValido()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Aluguel", "Despesa Aluguel",
            TipoBaseRateio.FixoPercentual,
            [new RegraDestinoDto(10, 60m, null), new RegraDestinoDto(20, 40m, null)]);

        Assert.True(_criarRegraValidator.Validate(cmd).IsValid);
    }

    [Fact]
    public void CriarRegraRateio_SomaPercentualDiferenteDe100_EhInvalido()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Aluguel", "Despesa Aluguel",
            TipoBaseRateio.FixoPercentual,
            [new RegraDestinoDto(10, 60m, null), new RegraDestinoDto(20, 30m, null)]);

        var result = _criarRegraValidator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("100%"));
    }

    [Fact]
    public void CriarRegraRateio_BaseDinamicaSemValorBase_EhInvalido()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Energia", "Despesa Energia",
            TipoBaseRateio.AreaM2,
            [new RegraDestinoDto(10, 50m, null), new RegraDestinoDto(20, 50m, 80m)]);

        var result = _criarRegraValidator.Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("ValorBase"));
    }

    [Fact]
    public void CriarRegraRateio_BaseDinamicaComValoresBase_EhValido()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Energia", "Despesa Energia",
            TipoBaseRateio.Headcount,
            [new RegraDestinoDto(10, 50m, 30m), new RegraDestinoDto(20, 50m, 10m)]);

        Assert.True(_criarRegraValidator.Validate(cmd).IsValid);
    }

    [Fact]
    public void CriarRegraRateio_SemDestinos_EhInvalido()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Aluguel", "Despesa Aluguel",
            TipoBaseRateio.FixoPercentual, []);

        Assert.False(_criarRegraValidator.Validate(cmd).IsValid);
    }

    // ---------- AtualizarOrcamentoCommandValidator ----------

    private readonly AtualizarOrcamentoCommandValidator _orcamentoValidator = new();

    [Theory]
    [InlineData(1, true)]
    [InlineData(12, true)]
    [InlineData(0, false)]
    [InlineData(13, false)]
    public void AtualizarOrcamento_ValidaIntervaloDoMes(int mes, bool esperado)
    {
        var cmd = new AtualizarOrcamentoCommand(5, "Energia", 2026, mes, 1000m);

        Assert.Equal(esperado, _orcamentoValidator.Validate(cmd).IsValid);
    }

    [Fact]
    public void AtualizarOrcamento_ValorNegativo_EhInvalido()
    {
        var cmd = new AtualizarOrcamentoCommand(5, "Energia", 2026, 7, -1m);

        Assert.False(_orcamentoValidator.Validate(cmd).IsValid);
    }

    // ---------- ExecutarRateioCommandValidator ----------

    private readonly ExecutarRateioCommandValidator _executarValidator = new();

    [Fact]
    public void ExecutarRateio_CompetenciaValida_EhValido()
    {
        Assert.True(_executarValidator.Validate(new ExecutarRateioCommand(1, 2026, 7)).IsValid);
    }

    [Theory]
    [InlineData(0, 2026, 7)]  // filial inválida
    [InlineData(1, 1999, 7)]  // ano fora do intervalo
    [InlineData(1, 2026, 0)]  // mês inválido
    public void ExecutarRateio_ParametrosInvalidos_EhInvalido(int cdFilial, int ano, int mes)
    {
        Assert.False(_executarValidator.Validate(new ExecutarRateioCommand(cdFilial, ano, mes)).IsValid);
    }
}
