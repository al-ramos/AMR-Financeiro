using Moq;
using AMR.Financeiro.Application.Features.CentroCusto.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.CentroCusto;

public class CriarRegraRateioHandlerTests
{
    private readonly Mock<ICentroCustoRepository> _repoMock = new();

    private CriarRegraRateioHandler CreateHandler() => new(_repoMock.Object);

    [Fact]
    public async Task Handle_SemDestinos_LancaInvalidOperationException()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Aluguel", "Despesa Aluguel",
            TipoBaseRateio.FixoPercentual, []);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("pelo menos um destino", ex.Message);
    }

    [Fact]
    public async Task Handle_SomaPercentualDiferenteDe100_LancaInvalidOperationException()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Aluguel", "Despesa Aluguel",
            TipoBaseRateio.FixoPercentual,
            [new RegraDestinoDto(10, 60m, null), new RegraDestinoDto(20, 30m, null)]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("100%", ex.Message);
        _repoMock.Verify(r => r.AddRegraAsync(It.IsAny<RegraRateio>(),
            It.IsAny<List<RegraRateioDestino>>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_Soma100ComTolerancia_AceitaTresDestinosDeUmTerco()
    {
        var cmd = new CriarRegraRateioCommand(1, "Rateio Energia", "Despesa Energia",
            TipoBaseRateio.FixoPercentual,
            [
                new RegraDestinoDto(10, 33.33m, null),
                new RegraDestinoDto(20, 33.33m, null),
                new RegraDestinoDto(30, 33.34m, null)
            ]);

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AddRegraAsync(It.IsAny<RegraRateio>(),
            It.Is<List<RegraRateioDestino>>(d => d.Count == 3), default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_CriaRegraComDestinos()
    {
        RegraRateio? regraCapturada = null;
        List<RegraRateioDestino>? destinosCapturados = null;
        _repoMock.Setup(r => r.AddRegraAsync(It.IsAny<RegraRateio>(),
                It.IsAny<List<RegraRateioDestino>>(), default))
            .Callback<RegraRateio, List<RegraRateioDestino>, CancellationToken>(
                (regra, destinos, _) => { regraCapturada = regra; destinosCapturados = destinos; })
            .Returns(Task.CompletedTask);

        var cmd = new CriarRegraRateioCommand(1, "Rateio Aluguel", "Despesa Aluguel",
            TipoBaseRateio.AreaM2,
            [new RegraDestinoDto(10, 60m, 150m), new RegraDestinoDto(20, 40m, 50m)]);

        await CreateHandler().Handle(cmd, default);

        Assert.NotNull(regraCapturada);
        Assert.Equal("Rateio Aluguel", regraCapturada!.Nome);
        Assert.Equal("Despesa Aluguel", regraCapturada.ContaOrigemDescricao);
        Assert.Equal(TipoBaseRateio.AreaM2, regraCapturada.TipoBase);
        Assert.True(regraCapturada.Ativo);

        Assert.NotNull(destinosCapturados);
        Assert.Equal(2, destinosCapturados!.Count);
        Assert.Equal(150m, destinosCapturados[0].ValorBase);
        Assert.Equal(20, destinosCapturados[1].CentroCustoId);
    }
}
