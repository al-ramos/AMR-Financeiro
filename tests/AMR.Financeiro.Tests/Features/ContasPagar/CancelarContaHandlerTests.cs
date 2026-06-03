using Moq;
using AMR.Financeiro.Application.Features.ContasPagar.Commands;
using AMR.Financeiro.Application.Features.ContasPagar.Handlers;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.ContasPagar;

public class CancelarContaHandlerTests
{
    private readonly Mock<IContaPagarRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CancelarContaHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static ContaPagar ContaAberta() =>
        new(1, "Fornecedor XYZ", 300m, new DateOnly(2026, 8, 1));

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((ContaPagar?)null);

        var result = await CreateHandler().Handle(new CancelarContaCommand(99), default);

        Assert.False(result);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaPagar>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaExistente_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(ContaAberta());

        var result = await CreateHandler().Handle(new CancelarContaCommand(1), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ContaExistente_StatusFicaCancelada()
    {
        var conta = ContaAberta();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(conta);

        await CreateHandler().Handle(new CancelarContaCommand(1), default);

        Assert.Equal(StatusConta.Cancelada, conta.Status);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaPagar>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
