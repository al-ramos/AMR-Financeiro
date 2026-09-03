using Moq;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.PlanoDeContas;

public class AtualizarContaHandlerTests
{
    private readonly Mock<IPlanoDeContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private AtualizarContaHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoDeContas ContaExistente() =>
        new(1, "4.1", "Despesas Administrativas", TipoContaContabil.Despesa,
            NaturezaConta.Devedora, 2, 1, GrupoDRE.DespesasOperacionais, 10);

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);

        var result = await CreateHandler().Handle(
            new AtualizarContaCommand(99, "Nova", GrupoDRE.DespesasOperacionais, 1), default);

        Assert.False(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaEncontrada_AtualizaDescricaoGrupoEOrdem()
    {
        var conta = ContaExistente();
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(conta);

        var result = await CreateHandler().Handle(
            new AtualizarContaCommand(1, "Despesas Gerais", GrupoDRE.DespesasFinanceiras, 20), default);

        Assert.True(result);
        Assert.Equal("Despesas Gerais", conta.Descricao);
        Assert.Equal(GrupoDRE.DespesasFinanceiras, conta.GrupoDRE);
        Assert.Equal(20, conta.OrdemExibicao);
        _repoMock.Verify(r => r.UpdateAsync(conta, default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}

public class InativarContaHandlerTests
{
    private readonly Mock<IPlanoDeContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private InativarContaHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoDeContas ContaAtiva() =>
        new(1, "3.1.1.1.1", "Venda de Produtos", TipoContaContabil.Receita,
            NaturezaConta.Credora, 5, 4, GrupoDRE.ReceitaBruta, 1);

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);

        var result = await CreateHandler().Handle(new InativarContaCommand(99), default);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ContaComLancamentos_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(ContaAtiva());
        _repoMock.Setup(r => r.TemLancamentosAsync(1, default)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(new InativarContaCommand(1), default));

        Assert.Contains("lançamentos", ex.Message);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaComFilhas_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(ContaAtiva());
        _repoMock.Setup(r => r.TemLancamentosAsync(1, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.TemContasFilhasAsync(1, default)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(new InativarContaCommand(1), default));

        Assert.Contains("filhas", ex.Message);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaSemVinculos_InativaESalva()
    {
        var conta = ContaAtiva();
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(conta);
        _repoMock.Setup(r => r.TemLancamentosAsync(1, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.TemContasFilhasAsync(1, default)).ReturnsAsync(false);

        var result = await CreateHandler().Handle(new InativarContaCommand(1), default);

        Assert.True(result);
        Assert.False(conta.Ativo);
        _repoMock.Verify(r => r.UpdateAsync(conta, default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
