using AMR.Financeiro.Application.Features.ContasPagar.Commands;
using AMR.Financeiro.Application.Features.ContasPagar.Handlers;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;
using Moq;
using Xunit;

namespace AMR.Financeiro.Tests.Features.ContasPagar;

public class CriarContaPagarHandlerTests
{
    private readonly Mock<IContaPagarRepository> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;

    public CriarContaPagarHandlerTests()
    {
        _repoMock = new Mock<IContaPagarRepository>();
        _uowMock = new Mock<IUnitOfWork>();
    }

    private CriarContaPagarHandler CreateHandler()
    {
        return new CriarContaPagarHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_ChamaRepositorio()
    {
        // Arrange
        var command = new CriarContaPagarCommand(1, "Fornecedor XYZ", 1000m, new DateOnly(2026, 6, 30));

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<ContaPagar>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExecutadoComSucesso_SalvaAlteracoes()
    {
        // Arrange
        var command = new CriarContaPagarCommand(1, "Fornecedor ABC", 500m, new DateOnly(2026, 7, 15));

        // Act
        await CreateHandler().Handle(command, default);

        // Assert
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_AdicionaNovaContaAoRepositorio()
    {
        // Arrange
        var command = new CriarContaPagarCommand(2, "Fornecedor DEF", 2000m, new DateOnly(2026, 8, 20));
        ContaPagar? capturedConta = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<ContaPagar>(), default))
            .Callback<ContaPagar, CancellationToken>((c, _) => capturedConta = c);

        // Act
        await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotNull(capturedConta);
        Assert.Equal(1000, capturedConta.Valor); // Valor deve estar no objeto
    }

    [Fact]
    public async Task Handle_VerificaDescricaoSalva()
    {
        // Arrange
        const string descricao = "NF-12345 Fornecedor XYZ";
        var command = new CriarContaPagarCommand(1, descricao, 1500m, new DateOnly(2026, 9, 10));
        ContaPagar? capturedConta = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<ContaPagar>(), default))
            .Callback<ContaPagar, CancellationToken>((c, _) => capturedConta = c);

        // Act
        await CreateHandler().Handle(command, default);

        // Assert
        Assert.NotNull(capturedConta);
        Assert.Contains(descricao, capturedConta.Descricao);
    }
}
