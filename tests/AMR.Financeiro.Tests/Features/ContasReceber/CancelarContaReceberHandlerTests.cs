using Moq;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Application.Features.ContasReceber.Handlers;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.ContasReceber;

public class CancelarContaReceberHandlerTests
{
    private readonly Mock<IContaReceberRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CancelarContaReceberHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static ContaReceber ContaAberta() =>
        new(1, "Cliente XYZ", 400m, new DateOnly(2026, 8, 1));

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((ContaReceber?)null);

        var result = await CreateHandler().Handle(new CancelarContaReceberCommand(99), default);

        Assert.False(result);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaReceber>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaExistente_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(ContaAberta());

        var result = await CreateHandler().Handle(new CancelarContaReceberCommand(1), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ContaExistente_StatusFicaCancelada()
    {
        var conta = ContaAberta();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(conta);

        await CreateHandler().Handle(new CancelarContaReceberCommand(1), default);

        Assert.Equal(StatusContaReceber.Cancelada, conta.Status);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaReceber>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
