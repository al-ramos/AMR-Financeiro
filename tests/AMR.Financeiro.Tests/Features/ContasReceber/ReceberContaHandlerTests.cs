using Moq;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Application.Features.ContasReceber.Handlers;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using TipoContaPlano = AMR.Financeiro.Domain.Enums.TipoContaPlano;

namespace AMR.Financeiro.Tests.Features.ContasReceber;

public class ReceberContaHandlerTests
{
    private readonly Mock<IContaReceberRepository> _repoMock = new();
    private readonly Mock<IPlanoContasRepository> _planoMock = new();
    private readonly Mock<ILancamentoFinanceiroRepository> _lancamentoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private ReceberContaHandler CreateHandler() =>
        new(_repoMock.Object, _planoMock.Object, _lancamentoMock.Object, _uowMock.Object);

    private static ContaReceber ContaAberta() =>
        new(1, "Cliente XYZ", 1000m, new DateOnly(2026, 6, 30));

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((ContaReceber?)null);

        var result = await CreateHandler().Handle(new ReceberContaCommand(99, new DateOnly(2026, 6, 1)), default);

        Assert.False(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaExistente_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(ContaAberta());

        var result = await CreateHandler().Handle(new ReceberContaCommand(1, new DateOnly(2026, 6, 1)), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_ContaExistente_StatusFicaRecebida()
    {
        var conta = ContaAberta();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(conta);

        await CreateHandler().Handle(new ReceberContaCommand(1, new DateOnly(2026, 6, 1)), default);

        Assert.Equal(StatusContaReceber.Recebida, conta.Status);
        Assert.Equal(new DateOnly(2026, 6, 1), conta.DataRecebimento);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaReceber>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ComPlanoConta_CriaLancamentoCredito()
    {
        var conta = ContaAberta();
        var plano = new AMR.Financeiro.Domain.Entities.PlanoContas(1, "1.1.3", "Contas a Receber", TipoContaPlano.Analitica);

        _repoMock.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(conta);
        _planoMock.Setup(r => r.ObterPorCodigoAsync(1, "1.1.3", default)).ReturnsAsync(plano);

        await CreateHandler().Handle(new ReceberContaCommand(1, new DateOnly(2026, 6, 1)), default);

        _lancamentoMock.Verify(r => r.AdicionarAsync(It.IsAny<LancamentoFinanceiro>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_SemPlanoConta_NaoCriaLancamento()
    {
        var conta = ContaAberta();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(conta);
        _planoMock.Setup(r => r.ObterPorCodigoAsync(1, "1.1.3", default))
                  .ReturnsAsync((AMR.Financeiro.Domain.Entities.PlanoContas?)null);

        await CreateHandler().Handle(new ReceberContaCommand(1, new DateOnly(2026, 6, 1)), default);

        _lancamentoMock.Verify(r => r.AdicionarAsync(It.IsAny<LancamentoFinanceiro>(), default), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
