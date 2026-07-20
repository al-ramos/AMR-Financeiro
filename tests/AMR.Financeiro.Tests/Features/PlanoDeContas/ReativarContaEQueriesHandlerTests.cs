using Moq;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;
using AMR.Financeiro.Application.Features.PlanoDeContas.Queries;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.PlanoDeContas;

public class ReativarContaHandlerTests
{
    private readonly Mock<IPlanoDeContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private ReativarContaHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoDeContas ContaInativa()
    {
        var c = new Domain.Entities.PlanoDeContas(
            1, "3.1.1.1.1", "Venda de Produtos", TipoContaContabil.Receita,
            NaturezaConta.Credora, 5, 4, GrupoDRE.ReceitaBruta, 1);
        c.Inativar();
        return c;
    }

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);

        var result = await CreateHandler().Handle(new ReativarContaCommand(99), default);

        Assert.False(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContaInativa_ReativaESalva()
    {
        var conta = ContaInativa();
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(conta);

        var result = await CreateHandler().Handle(new ReativarContaCommand(1), default);

        Assert.True(result);
        Assert.True(conta.Ativo);
        _repoMock.Verify(r => r.UpdateAsync(conta, default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}

public class GetContaByIdHandlerTests
{
    private readonly Mock<IPlanoDeContasRepository> _repoMock = new();

    private GetContaByIdHandler CreateHandler() => new(_repoMock.Object);

    [Fact]
    public async Task Handle_ContaNaoEncontrada_RetornaNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);

        var result = await CreateHandler().Handle(new GetContaByIdQuery(99), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ContaEncontrada_RetornaDtoMapeado()
    {
        var conta = new Domain.Entities.PlanoDeContas(
            1, "4.1", "Despesas Administrativas", TipoContaContabil.Despesa,
            NaturezaConta.Devedora, 2, 3, GrupoDRE.DespesasOperacionais, 10);
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(conta);

        var dto = await CreateHandler().Handle(new GetContaByIdQuery(1), default);

        Assert.NotNull(dto);
        Assert.Equal("4.1", dto!.Codigo);
        Assert.Equal("Despesas Administrativas", dto.Descricao);
        Assert.Equal(TipoContaContabil.Despesa, dto.Tipo);
        Assert.Equal(NaturezaConta.Devedora, dto.Natureza);
        Assert.Equal(2, dto.Nivel);
        Assert.Equal(3, dto.PaiId);
        Assert.Equal(GrupoDRE.DespesasOperacionais, dto.GrupoDRE);
        Assert.Equal(10, dto.OrdemExibicao);
        Assert.True(dto.Ativo);
    }
}

public class GetPlanoContasArvoreHandlerTests
{
    private readonly Mock<IPlanoDeContasRepository> _repoMock = new();

    private GetPlanoContasArvoreHandler CreateHandler() => new(_repoMock.Object);

    private static Domain.Entities.PlanoDeContas Conta(
        string codigo, int nivel, int? paiId, int ordem, int id)
    {
        var c = new Domain.Entities.PlanoDeContas(
            1, codigo, $"Conta {codigo}", TipoContaContabil.Receita,
            NaturezaConta.Credora, nivel, paiId, GrupoDRE.ReceitaBruta, ordem);
        typeof(Domain.Entities.PlanoDeContas).GetProperty(nameof(c.Id))!
            .SetValue(c, id);
        return c;
    }

    [Fact]
    public async Task Handle_SemContas_RetornaArvoreVazia()
    {
        _repoMock.Setup(r => r.GetByCdFilialAsync(1, false, default))
                 .ReturnsAsync([]);

        var arvore = await CreateHandler().Handle(new GetPlanoContasArvoreQuery(1), default);

        Assert.Empty(arvore);
    }

    [Fact]
    public async Task Handle_ContasHierarquicas_MontaArvoreComFilhos()
    {
        var raiz = Conta("3", 1, null, 1, 1);
        var filha = Conta("3.1", 2, 1, 1, 2);
        var neta = Conta("3.1.1", 3, 2, 1, 3);
        _repoMock.Setup(r => r.GetByCdFilialAsync(1, false, default))
                 .ReturnsAsync([raiz, filha, neta]);

        var arvore = await CreateHandler().Handle(new GetPlanoContasArvoreQuery(1), default);

        var noRaiz = Assert.Single(arvore);
        Assert.Equal("3", noRaiz.Codigo);
        var noFilha = Assert.Single(noRaiz.Filhos);
        Assert.Equal("3.1", noFilha.Codigo);
        var noNeta = Assert.Single(noFilha.Filhos);
        Assert.Equal("3.1.1", noNeta.Codigo);
        Assert.Empty(noNeta.Filhos);
    }

    [Fact]
    public async Task Handle_IrmasOrdenadasPorOrdemExibicaoDepoisCodigo()
    {
        var raiz = Conta("3", 1, null, 1, 1);
        var segunda = Conta("3.2", 2, 1, 2, 2);
        var primeira = Conta("3.1", 2, 1, 1, 3);
        _repoMock.Setup(r => r.GetByCdFilialAsync(1, false, default))
                 .ReturnsAsync([raiz, segunda, primeira]);

        var arvore = await CreateHandler().Handle(new GetPlanoContasArvoreQuery(1), default);

        var filhos = arvore[0].Filhos;
        Assert.Equal(["3.1", "3.2"], filhos.Select(f => f.Codigo).ToArray());
    }
}
