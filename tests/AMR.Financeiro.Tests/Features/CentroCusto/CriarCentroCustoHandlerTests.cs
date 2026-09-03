using Moq;
using AMR.Financeiro.Application.Features.CentroCusto.Commands;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using CentroCustoEntity = AMR.Financeiro.Domain.Entities.CentroCusto;

namespace AMR.Financeiro.Tests.Features.CentroCusto;

public class CriarCentroCustoHandlerTests
{
    private readonly Mock<ICentroCustoRepository> _repoMock = new();

    private CriarCentroCustoHandler CreateHandler() => new(_repoMock.Object);

    [Fact]
    public async Task Handle_PaiNaoEncontrado_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((CentroCustoEntity?)null);

        var cmd = new CriarCentroCustoCommand(1, "1.1", "Produção", TipoCentroCusto.Produtivo,
            99, 2, "Alessandro");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("99", ex.Message);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<CentroCustoEntity>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_NivelForaDoIntervalo_LancaArgumentOutOfRangeException()
    {
        var cmd = new CriarCentroCustoCommand(1, "1", "Fábrica", TipoCentroCusto.Produtivo,
            null, 4, "Alessandro");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateHandler().Handle(cmd, default));

        _repoMock.Verify(r => r.AddAsync(It.IsAny<CentroCustoEntity>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_DadosValidosSemPai_AdicionaCentroCusto()
    {
        CentroCustoEntity? capturado = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CentroCustoEntity>(), default))
                 .Callback<CentroCustoEntity, CancellationToken>((cc, _) => capturado = cc)
                 .Returns(Task.CompletedTask);

        var cmd = new CriarCentroCustoCommand(1, "1", "Fábrica", TipoCentroCusto.Produtivo,
            null, 1, "Alessandro");

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<CentroCustoEntity>(), default), Times.Once);
        Assert.NotNull(capturado);
        Assert.Equal(1, capturado!.CdFilial);
        Assert.Equal("1", capturado.Codigo);
        Assert.Equal("Fábrica", capturado.Descricao);
        Assert.Equal(TipoCentroCusto.Produtivo, capturado.Tipo);
        Assert.Null(capturado.PaiId);
        Assert.Equal(1, capturado.Nivel);
        Assert.Equal("Alessandro", capturado.ResponsavelNome);
        Assert.True(capturado.Ativo);
    }

    [Fact]
    public async Task Handle_ComPaiExistente_AdicionaCentroCustoVinculado()
    {
        var pai = new CentroCustoEntity(1, "1", "Fábrica", TipoCentroCusto.Produtivo, null, 1, "Alessandro");
        _repoMock.Setup(r => r.GetByIdAsync(7, default)).ReturnsAsync(pai);

        CentroCustoEntity? capturado = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CentroCustoEntity>(), default))
                 .Callback<CentroCustoEntity, CancellationToken>((cc, _) => capturado = cc)
                 .Returns(Task.CompletedTask);

        var cmd = new CriarCentroCustoCommand(1, "1.2", "Usinagem", TipoCentroCusto.Produtivo,
            7, 2, "Marcos");

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<CentroCustoEntity>(), default), Times.Once);
        Assert.Equal(7, capturado!.PaiId);
        Assert.Equal(2, capturado.Nivel);
    }
}
