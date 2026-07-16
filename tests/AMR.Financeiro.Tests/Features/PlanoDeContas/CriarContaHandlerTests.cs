using Moq;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;
using AMR.Financeiro.Application.Features.PlanoDeContas.Validators;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.PlanoDeContas;

public class CriarContaHandlerTests
{
    private readonly Mock<IPlanoDeContasRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CriarContaHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    private static Domain.Entities.PlanoDeContas Conta(
        string codigo = "3", int nivel = 1, int? paiId = null,
        GrupoDRE grupo = GrupoDRE.ReceitaBruta) =>
        new(1, codigo, "Conta Teste", TipoContaContabil.Receita, NaturezaConta.Credora, nivel, paiId, grupo, 1);

    [Fact]
    public async Task Handle_CodigoDuplicado_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(1, "3.1", default))
                 .ReturnsAsync(Conta("3.1", 2, 1));

        var cmd = new CriarContaCommand(1, "3.1", "Receita de Vendas",
            TipoContaContabil.Receita, NaturezaConta.Credora, 2, 1, GrupoDRE.ReceitaBruta, 1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("3.1", ex.Message);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.PlanoDeContas>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_PaiInformadoNaoExiste_LancaInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(1, "3.1", default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);
        _repoMock.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);

        var cmd = new CriarContaCommand(1, "3.1", "Receita de Vendas",
            TipoContaContabil.Receita, NaturezaConta.Credora, 2, 99, GrupoDRE.ReceitaBruta, 1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task Handle_DadosValidos_AdicionaESalva()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(1, "3", default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);

        var cmd = new CriarContaCommand(1, "3", "Receitas",
            TipoContaContabil.Receita, NaturezaConta.Credora, 1, null, GrupoDRE.ReceitaBruta, 1);

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<Domain.Entities.PlanoDeContas>(c => c.Codigo == "3" && c.Nivel == 1),
            default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ContaNivel5_AceitaLancamentos()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(1, "3.1.1.1.1", default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);
        _repoMock.Setup(r => r.GetByIdAsync(4, default))
                 .ReturnsAsync(Conta("3.1.1.1", 4, 3));

        Domain.Entities.PlanoDeContas? capturada = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.PlanoDeContas>(), default))
                 .Callback<Domain.Entities.PlanoDeContas, CancellationToken>((c, _) => capturada = c);

        var cmd = new CriarContaCommand(1, "3.1.1.1.1", "Venda de Produtos",
            TipoContaContabil.Receita, NaturezaConta.Credora, 5, 4, GrupoDRE.ReceitaBruta, 1);

        await CreateHandler().Handle(cmd, default);

        Assert.NotNull(capturada);
        Assert.True(capturada!.AceitaLancamentos);
    }

    [Fact]
    public async Task Handle_ContaNivelIntermediario_NaoAceitaLancamentos()
    {
        _repoMock.Setup(r => r.GetByCodigoAsync(1, "3.1", default))
                 .ReturnsAsync((Domain.Entities.PlanoDeContas?)null);
        _repoMock.Setup(r => r.GetByIdAsync(1, default))
                 .ReturnsAsync(Conta());

        Domain.Entities.PlanoDeContas? capturada = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.PlanoDeContas>(), default))
                 .Callback<Domain.Entities.PlanoDeContas, CancellationToken>((c, _) => capturada = c);

        var cmd = new CriarContaCommand(1, "3.1", "Receita Operacional",
            TipoContaContabil.Receita, NaturezaConta.Credora, 2, 1, GrupoDRE.ReceitaBruta, 1);

        await CreateHandler().Handle(cmd, default);

        Assert.NotNull(capturada);
        Assert.False(capturada!.AceitaLancamentos);
    }

    [Fact]
    public void Entidade_NivelForaDoIntervalo_LancaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Domain.Entities.PlanoDeContas(
                1, "3.1.1.1.1.1", "Nível 6 inválido",
                TipoContaContabil.Receita, NaturezaConta.Credora, 6, 5, GrupoDRE.ReceitaBruta, 1));
    }
}

public class CriarContaCommandValidatorTests
{
    private readonly CriarContaCommandValidator _validator = new();

    private static CriarContaCommand Cmd(
        string codigo = "3.1", int nivel = 2, int? paiId = 1,
        string descricao = "Receita de Vendas", int ordem = 1) =>
        new(1, codigo, descricao, TipoContaContabil.Receita, NaturezaConta.Credora,
            nivel, paiId, GrupoDRE.ReceitaBruta, ordem);

    [Fact]
    public void Validate_ComandoValido_Passa() =>
        Assert.True(_validator.Validate(Cmd()).IsValid);

    [Fact]
    public void Validate_CodigoCincoNiveis_Passa() =>
        Assert.True(_validator.Validate(Cmd("3.1.1.1.1", 5, 4)).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("3.")]
    [InlineData("a.b")]
    [InlineData("3.1.1.1.1.1")] // 6 níveis
    public void Validate_CodigoInvalido_Falha(string codigo) =>
        Assert.False(_validator.Validate(Cmd(codigo, 2, 1)).IsValid);

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_NivelForaDoIntervalo_Falha(int nivel) =>
        Assert.False(_validator.Validate(Cmd("3.1", nivel, 1)).IsValid);

    [Fact]
    public void Validate_NivelNaoCorrespondeAoCodigo_Falha() =>
        // Código 3.1.1 tem profundidade 3, mas o nível informado é 2
        Assert.False(_validator.Validate(Cmd("3.1.1", 2, 1)).IsValid);

    [Fact]
    public void Validate_NivelMaiorQueUmSemPai_Falha() =>
        Assert.False(_validator.Validate(Cmd("3.1", 2, null)).IsValid);

    [Fact]
    public void Validate_NivelUmComPai_Falha() =>
        Assert.False(_validator.Validate(Cmd("3", 1, 1)).IsValid);

    [Fact]
    public void Validate_DescricaoVazia_Falha() =>
        Assert.False(_validator.Validate(Cmd(descricao: "")).IsValid);

    [Fact]
    public void Validate_OrdemExibicaoNegativa_Falha() =>
        Assert.False(_validator.Validate(Cmd(ordem: -1)).IsValid);
}
