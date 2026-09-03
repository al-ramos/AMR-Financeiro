using Moq;
using AMR.Financeiro.Application.Features.CentroCusto.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using CentroCustoEntity = AMR.Financeiro.Domain.Entities.CentroCusto;

namespace AMR.Financeiro.Tests.Features.CentroCusto;

public class AtualizarOrcamentoHandlerTests
{
    private readonly Mock<ICentroCustoRepository> _repoMock = new();

    private AtualizarOrcamentoHandler CreateHandler() => new(_repoMock.Object);

    private static CentroCustoEntity CriarCentroCusto() =>
        new(1, "1.1", "TI", TipoCentroCusto.Administrativo, null, 2, "Alessandro");

    [Fact]
    public async Task Handle_CentroCustoNaoEncontrado_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(5, default))
                 .ReturnsAsync((CentroCustoEntity?)null);

        var cmd = new AtualizarOrcamentoCommand(5, "Energia Elétrica", 2026, 7, 10_000m);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        _repoMock.Verify(r => r.UpsertOrcamentoAsync(It.IsAny<OrcamentoCC>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_MesInvalido_LancaArgumentOutOfRangeException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(CriarCentroCusto());

        var cmd = new AtualizarOrcamentoCommand(5, "Energia Elétrica", 2026, 13, 10_000m);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateHandler().Handle(cmd, default));
    }

    [Fact]
    public async Task Handle_DadosValidos_ChamaUpsertComValoresCorretos()
    {
        _repoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(CriarCentroCusto());

        OrcamentoCC? capturado = null;
        _repoMock.Setup(r => r.UpsertOrcamentoAsync(It.IsAny<OrcamentoCC>(), default))
                 .Callback<OrcamentoCC, CancellationToken>((o, _) => capturado = o)
                 .Returns(Task.CompletedTask);

        var cmd = new AtualizarOrcamentoCommand(5, "Energia Elétrica", 2026, 7, 10_000m);

        var resultado = await CreateHandler().Handle(cmd, default);

        Assert.True(resultado);
        _repoMock.Verify(r => r.UpsertOrcamentoAsync(It.IsAny<OrcamentoCC>(), default), Times.Once);
        Assert.NotNull(capturado);
        Assert.Equal(5, capturado!.CentroCustoId);
        Assert.Equal("Energia Elétrica", capturado.ContaDescricao);
        Assert.Equal(2026, capturado.Ano);
        Assert.Equal(7, capturado.Mes);
        Assert.Equal(10_000m, capturado.ValorOrcado);
        Assert.Equal(0m, capturado.ValorRealizado);
    }
}
