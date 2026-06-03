using Moq;
using AMR.Financeiro.Application.Features.PlanoContas.Commands;
using AMR.Financeiro.Application.Features.PlanoContas.Handlers;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.PlanoContas;

public class CriarPlanoContasHandlerTests
{
    private readonly Mock<IPlanoContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CriarPlanoContasHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_CodigoDuplicado_LancaInvalidOperationException()
    {
        var existente = new Domain.Entities.PlanoContas(1, "1.1", "Duplicada", TipoContaPlano.Analitica);
        _repoMock.Setup(r => r.ObterPorCodigoAsync(1, "1.1", default))
                 .ReturnsAsync(existente);

        var cmd = new CriarPlanoContasCommand(1, "1.1", "Nova Conta", TipoContaPlano.Analitica, null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("1.1", ex.Message);
    }

    [Fact]
    public async Task Handle_PaiInformadoNaoExiste_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.ObterPorCodigoAsync(1, "1.1.1", default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        var cmd = new CriarPlanoContasCommand(1, "1.1.1", "Sub-conta", TipoContaPlano.Analitica, 99);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task Handle_DadosValidos_SemPai_AdicionaESalva()
    {
        _repoMock.Setup(r => r.ObterPorCodigoAsync(1, "2", default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        var cmd = new CriarPlanoContasCommand(1, "2", "Passivo", TipoContaPlano.Sintetica, null);

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Domain.Entities.PlanoContas>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_ComPai_AdicionaESalva()
    {
        var pai = new Domain.Entities.PlanoContas(1, "1", "Ativo", TipoContaPlano.Sintetica);
        _repoMock.Setup(r => r.ObterPorCodigoAsync(1, "1.1", default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(pai);

        var cmd = new CriarPlanoContasCommand(1, "1.1", "Ativo Circulante", TipoContaPlano.Sintetica, 1);

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AdicionarAsync(
            It.Is<Domain.Entities.PlanoContas>(p => p.PaiId == 1 && p.Codigo == "1.1"),
            default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_ContaCriadaFicaAtiva()
    {
        _repoMock.Setup(r => r.ObterPorCodigoAsync(1, "3", default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        Domain.Entities.PlanoContas? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Domain.Entities.PlanoContas>(), default))
                 .Callback<Domain.Entities.PlanoContas, CancellationToken>((p, _) => capturado = p);

        await CreateHandler().Handle(new CriarPlanoContasCommand(1, "3", "Resultado", TipoContaPlano.Sintetica, null), default);

        Assert.NotNull(capturado);
        Assert.True(capturado!.Ativo);
    }

    [Fact]
    public async Task Handle_ContaAnalitica_AceitaLancamentos()
    {
        _repoMock.Setup(r => r.ObterPorCodigoAsync(1, "1.1.1", default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        Domain.Entities.PlanoContas? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Domain.Entities.PlanoContas>(), default))
                 .Callback<Domain.Entities.PlanoContas, CancellationToken>((p, _) => capturado = p);

        await CreateHandler().Handle(new CriarPlanoContasCommand(1, "1.1.1", "Caixa", TipoContaPlano.Analitica, null), default);

        Assert.NotNull(capturado);
        Assert.True(capturado!.AceitaLancamentos());
    }
}

public class AtualizarPlanoContasHandlerTests
{
    private readonly Mock<IPlanoContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private AtualizarPlanoContasHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoContas PlanoExistente() =>
        new(1, "1.1.1", "Caixa Antigo", TipoContaPlano.Analitica);

    [Fact]
    public async Task Handle_PlanoNaoEncontrado_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        var result = await CreateHandler().Handle(new AtualizarPlanoContasCommand(99, "Nova Descricao"), default);

        Assert.False(result);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<Domain.Entities.PlanoContas>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_PlanoEncontrado_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(PlanoExistente());

        var result = await CreateHandler().Handle(new AtualizarPlanoContasCommand(1, "Caixa Principal"), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_PlanoEncontrado_AtualizaDescricaoESalva()
    {
        var plano = PlanoExistente();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(plano);

        await CreateHandler().Handle(new AtualizarPlanoContasCommand(1, "Caixa Principal"), default);

        Assert.Equal("Caixa Principal", plano.Descricao);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<Domain.Entities.PlanoContas>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_PlanoInativo_AtualizaDescricaoMesmoAssim()
    {
        var plano = PlanoExistente();
        plano.Inativar();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(plano);

        var result = await CreateHandler().Handle(new AtualizarPlanoContasCommand(1, "Descricao Nova"), default);

        Assert.True(result);
        Assert.Equal("Descricao Nova", plano.Descricao);
        Assert.False(plano.Ativo);
    }
}

public class InativarPlanoContasHandlerTests
{
    private readonly Mock<IPlanoContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private InativarPlanoContasHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoContas PlanoAtivo() =>
        new(1, "1.1.1", "Caixa", TipoContaPlano.Analitica);

    [Fact]
    public async Task Handle_PlanoNaoEncontrado_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        var result = await CreateHandler().Handle(new InativarPlanoContasCommand(99), default);

        Assert.False(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_PlanoEncontrado_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(PlanoAtivo());

        var result = await CreateHandler().Handle(new InativarPlanoContasCommand(1), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_PlanoAtivo_FicaInativo()
    {
        var plano = PlanoAtivo();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(plano);

        await CreateHandler().Handle(new InativarPlanoContasCommand(1), default);

        Assert.False(plano.Ativo);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<Domain.Entities.PlanoContas>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_PlanoInativado_NaoAceitaLancamentos()
    {
        var plano = PlanoAtivo();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(plano);

        await CreateHandler().Handle(new InativarPlanoContasCommand(1), default);

        Assert.False(plano.Ativo);
    }
}

public class AtivarPlanoContasHandlerTests
{
    private readonly Mock<IPlanoContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private AtivarPlanoContasHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoContas PlanoInativo()
    {
        var p = new Domain.Entities.PlanoContas(1, "1.1.1", "Caixa", TipoContaPlano.Analitica);
        p.Inativar();
        return p;
    }

    [Fact]
    public async Task Handle_PlanoNaoEncontrado_RetornaFalse()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoContas?)null);

        var result = await CreateHandler().Handle(new AtivarPlanoContasCommand(99), default);

        Assert.False(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_PlanoEncontrado_RetornaTrue()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(PlanoInativo());

        var result = await CreateHandler().Handle(new AtivarPlanoContasCommand(1), default);

        Assert.True(result);
    }

    [Fact]
    public async Task Handle_PlanoInativo_FicaAtivo()
    {
        var plano = PlanoInativo();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(plano);

        await CreateHandler().Handle(new AtivarPlanoContasCommand(1), default);

        Assert.True(plano.Ativo);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<Domain.Entities.PlanoContas>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_PlanoAtivado_AceitaLancamentosSeAnalitico()
    {
        var plano = PlanoInativo();
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default))
                 .ReturnsAsync(plano);

        await CreateHandler().Handle(new AtivarPlanoContasCommand(1), default);

        Assert.True(plano.Ativo);
        Assert.True(plano.AceitaLancamentos());
    }
}