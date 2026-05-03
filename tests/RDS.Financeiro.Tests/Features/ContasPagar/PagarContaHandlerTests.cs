using Moq;
using RDS.Financeiro.Application.Features.ContasPagar.Commands;
using RDS.Financeiro.Application.Features.ContasPagar.Handlers;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Enums;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Tests.Features.ContasPagar;

public class PagarContaHandlerTests
{
    private readonly Mock<IContaPagarRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private PagarContaHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static ContaPagar ContaAberta() =>
        new(1, "Fornecedor XYZ", 1000m, new DateOnly(2026, 5, 31));

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((ContaPagar?)null);

        var result = await CreateHandler().Handle(new PagarContaCommand(99, new DateOnly(2026, 5, 3)), default);

        Assert.False(result);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaPagar>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaExistente_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(ContaAberta());

        var result = await CreateHandler().Handle(new PagarContaCommand(1, new DateOnly(2026, 5, 3)), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ContaExistente_ChamaAtualizarESalva()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(ContaAberta());

        await CreateHandler().Handle(new PagarContaCommand(1, new DateOnly(2026, 5, 3)), default);

        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaPagar>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ContaExistente_StatusFicaPaga()
    {
        var conta = ContaAberta();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(conta);

        await CreateHandler().Handle(new PagarContaCommand(1, new DateOnly(2026, 5, 3)), default);

        Assert.Equal(StatusConta.Paga, conta.Status);
        Assert.Equal(new DateOnly(2026, 5, 3), conta.DataPagamento);
    }
}
