using Moq;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Services;

namespace AMR.Financeiro.Tests.Features.CentroCusto;

/// <summary>
/// Testes do algoritmo de rateio (Card 23.5).
///
/// O valor rateado sai dos lançamentos da conta de origem na competência — aqui
/// `ObterTotalDaContaAsync` é mockado. Antes o serviço usava R$ 1.000 fixos, e estes
/// testes afirmavam esse valor: eles codificavam o defeito em vez de pegá-lo. Ver FIN-02.
/// </summary>
public class RateioServiceTests
{
    /// <summary>Total apurado na conta de origem — o que o repositório devolve.</summary>
    private const decimal TotalDaConta = 1000m;
    private const int ContaOrigemId = 42;
    private static readonly DateOnly Competencia = new(2026, 7, 1);

    private readonly Mock<ICentroCustoRepository> _repoMock = new();

    private RateioService CreateService(decimal? total = TotalDaConta)
    {
        _repoMock.Setup(r => r.ObterTotalDaContaAsync(1, ContaOrigemId, Competencia, default))
                 .ReturnsAsync(total);
        return new(_repoMock.Object);
    }

    private static RegraRateio CriarRegra(TipoBaseRateio tipoBase,
        params (int ccId, decimal percentual, decimal? valorBase)[] destinos)
    {
        var regra = new RegraRateio(1, "Regra Teste", ContaOrigemId, "5.2.2 - Aluguel", tipoBase);
        foreach (var (ccId, percentual, valorBase) in destinos)
            regra.Destinos.Add(new RegraRateioDestino(0, ccId, percentual, valorBase));
        return regra;
    }

    [Fact]
    public async Task ExecutarMes_CompetenciaJaExecutada_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.RateioJaExecutadoAsync(1, Competencia, default))
                 .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService().ExecutarMesAsync(1, Competencia));

        Assert.Contains("já executado", ex.Message);
        _repoMock.Verify(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarMes_SemRegrasAtivas_RetornaZerosSemPersistir()
    {
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([]);

        var result = await CreateService().ExecutarMesAsync(1, Competencia);

        Assert.Equal(0, result.TotalRegras);
        Assert.Equal(0, result.TotalRateios);
        Assert.Equal(0m, result.ValorTotalRateado);
        Assert.Empty(result.Erros);
        _repoMock.Verify(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarMes_PercentualFixo_DistribuiConformePercentuais()
    {
        var regra = CriarRegra(TipoBaseRateio.FixoPercentual, (10, 60m, null), (20, 40m, null));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        List<RateioRealizado>? capturados = null;
        _repoMock.Setup(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default))
                 .Callback<List<RateioRealizado>, CancellationToken>((l, _) => capturados = l)
                 .Returns(Task.CompletedTask);

        var result = await CreateService().ExecutarMesAsync(1, Competencia);

        Assert.Equal(1, result.TotalRegras);
        Assert.Equal(2, result.TotalRateios);
        Assert.Equal(TotalDaConta, result.ValorTotalRateado);
        Assert.Empty(result.Erros);

        Assert.NotNull(capturados);
        var cc10 = capturados!.Single(r => r.CentroCustoId == 10);
        var cc20 = capturados.Single(r => r.CentroCustoId == 20);
        Assert.Equal(600m, cc10.ValorRateado);
        Assert.Equal(60m, cc10.PercentualAplicado);
        Assert.Equal(400m, cc20.ValorRateado);
        Assert.Equal(40m, cc20.PercentualAplicado);
        Assert.All(capturados, r => Assert.Equal(Competencia, r.Competencia));
    }

    [Fact]
    public async Task ExecutarMes_BaseAreaM2_RecalculaPercentualProporcionalAArea()
    {
        // 150 m² e 50 m² → 75% e 25%, independentemente dos percentuais informados
        var regra = CriarRegra(TipoBaseRateio.AreaM2, (10, 50m, 150m), (20, 50m, 50m));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        List<RateioRealizado>? capturados = null;
        _repoMock.Setup(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default))
                 .Callback<List<RateioRealizado>, CancellationToken>((l, _) => capturados = l)
                 .Returns(Task.CompletedTask);

        var result = await CreateService().ExecutarMesAsync(1, Competencia);

        Assert.Equal(2, result.TotalRateios);
        Assert.Equal(TotalDaConta, result.ValorTotalRateado);

        var cc10 = capturados!.Single(r => r.CentroCustoId == 10);
        var cc20 = capturados.Single(r => r.CentroCustoId == 20);
        Assert.Equal(750m, cc10.ValorRateado);
        Assert.Equal(75m, cc10.PercentualAplicado);
        Assert.Equal(250m, cc20.ValorRateado);
        Assert.Equal(25m, cc20.PercentualAplicado);
    }

    [Fact]
    public async Task ExecutarMes_BaseHeadcount_RecalculaPercentualProporcionalAoHeadcount()
    {
        // 30 e 10 pessoas → 75% e 25%
        var regra = CriarRegra(TipoBaseRateio.Headcount, (10, 50m, 30m), (20, 50m, 10m));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        List<RateioRealizado>? capturados = null;
        _repoMock.Setup(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default))
                 .Callback<List<RateioRealizado>, CancellationToken>((l, _) => capturados = l)
                 .Returns(Task.CompletedTask);

        await CreateService().ExecutarMesAsync(1, Competencia);

        Assert.Equal(750m, capturados!.Single(r => r.CentroCustoId == 10).ValorRateado);
        Assert.Equal(250m, capturados.Single(r => r.CentroCustoId == 20).ValorRateado);
    }

    [Fact]
    public async Task ExecutarMes_BaseDinamicaSemValorBase_NaoRateiaERegistraErro()
    {
        // Base dinamica sem ValorBase nao tem como ser proporcional. Antes caia nos
        // percentuais fixos em silencio, aplicando um criterio diferente do da regra.
        var regra = CriarRegra(TipoBaseRateio.AreaM2, (10, 50m, null), (20, 50m, null));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        var result = await CreateService().ExecutarMesAsync(1, Competencia);

        Assert.Equal(0, result.TotalRateios);
        Assert.Equal(0m, result.ValorTotalRateado);
        Assert.Single(result.Erros);
        Assert.Contains("sem ValorBase", result.Erros[0]);
        _repoMock.Verify(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarMes_ContaSemLancamentoNoMes_NaoRateiaERegistraErro()
    {
        var regra = CriarRegra(TipoBaseRateio.FixoPercentual, (10, 100m, null));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        var result = await CreateService(total: 0m).ExecutarMesAsync(1, Competencia);

        Assert.Equal(0, result.TotalRateios);
        Assert.Single(result.Erros);
        Assert.Contains("nao teve lancamento", result.Erros[0]);
        _repoMock.Verify(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarMes_ContaOrigemInexistente_NaoRateiaERegistraErro()
    {
        var regra = CriarRegra(TipoBaseRateio.FixoPercentual, (10, 100m, null));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        var result = await CreateService(total: null).ExecutarMesAsync(1, Competencia);

        Assert.Equal(0, result.TotalRateios);
        Assert.Single(result.Erros);
        Assert.Contains("nao encontrada", result.Erros[0]);
        _repoMock.Verify(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default), Times.Never);
    }

    [Fact]
    public async Task ExecutarMes_TotalApuradoMuda_ValorRateadoAcompanha()
    {
        // O ponto do FIN-02: o resultado segue a apuracao, nao uma constante.
        var regra = CriarRegra(TipoBaseRateio.FixoPercentual, (10, 100m, null));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regra]);

        List<RateioRealizado>? capturados = null;
        _repoMock.Setup(r => r.AddRateiosAsync(It.IsAny<List<RateioRealizado>>(), default))
                 .Callback<List<RateioRealizado>, CancellationToken>((l, _) => capturados = l)
                 .Returns(Task.CompletedTask);

        await CreateService(total: 7350.45m).ExecutarMesAsync(1, Competencia);

        Assert.Equal(7350.45m, capturados!.Single(r => r.CentroCustoId == 10).ValorRateado);
    }

    [Fact]
    public async Task ExecutarMes_MultiplasRegras_SomaValorTotalRateado()
    {
        var regraA = CriarRegra(TipoBaseRateio.FixoPercentual, (10, 100m, null));
        var regraB = CriarRegra(TipoBaseRateio.Headcount, (10, 50m, 20m), (20, 50m, 20m));
        _repoMock.Setup(r => r.GetRegrasAtivasAsync(1, default)).ReturnsAsync([regraA, regraB]);

        var result = await CreateService().ExecutarMesAsync(1, Competencia);

        Assert.Equal(2, result.TotalRegras);
        Assert.Equal(3, result.TotalRateios);
        Assert.Equal(2 * TotalDaConta, result.ValorTotalRateado);
        _repoMock.Verify(r => r.AddRateiosAsync(
            It.Is<List<RateioRealizado>>(l => l.Count == 3), default), Times.Once);
    }
}
