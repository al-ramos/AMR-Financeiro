using Moq;
using AMR.Financeiro.Application.Features.DRE.Queries;
using AMR.Financeiro.Application.Features.DRE.Validators;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Tests.Features.DRE;

public class GetDreHandlerTests
{
    private readonly Mock<IDreService> _dreServiceMock = new();

    private GetDreHandler CreateHandler() => new(_dreServiceMock.Object);

    private static DreResult DreVazia(string periodo = "07/2026") =>
        new(periodo, new List<LinhasDRE>(), 0, 0, 0);

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task Handle_MesInvalido_LancaArgumentOutOfRangeException(int mes)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateHandler().Handle(new GetDreQuery(1, 2026, mes), default));

        _dreServiceMock.Verify(
            s => s.CalcularAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PeriodoValido_DelegaParaOServicoComOsParametros()
    {
        var esperado = DreVazia();
        _dreServiceMock.Setup(s => s.CalcularAsync(1, 2026, 7, default))
                       .ReturnsAsync(esperado);

        var result = await CreateHandler().Handle(new GetDreQuery(1, 2026, 7), default);

        Assert.Same(esperado, result);
        _dreServiceMock.Verify(s => s.CalcularAsync(1, 2026, 7, default), Times.Once);
    }

    [Fact]
    public async Task Handle_ResultadoDoServico_EhRetornadoIntacto()
    {
        var linhas = new List<LinhasDRE>
        {
            new(GrupoDRE.ReceitaBruta, "Receita Bruta", 1000m, 800m, 900m, 25m, 11.11m,
                Negrito: false, EhSubtotal: false,
                Contas: new List<ContaDRE> { new("3.1.1.1.1", "Venda de Produtos", 1000m) }),
            new(GrupoDRE.LucroLiquido, "(=) Lucro Líquido do Período", 150m, 100m, 120m, 50m, 25m,
                Negrito: true, EhSubtotal: true, Contas: new List<ContaDRE>()),
        };
        _dreServiceMock.Setup(s => s.CalcularAsync(1, 2026, 7, default))
                       .ReturnsAsync(new DreResult("07/2026", linhas, 40m, 20m, 15m));

        var result = await CreateHandler().Handle(new GetDreQuery(1, 2026, 7), default);

        Assert.Equal("07/2026", result.Periodo);
        Assert.Equal(2, result.Linhas.Count);
        Assert.Equal(15m, result.MargemLiquida);
        Assert.True(result.Linhas[1].EhSubtotal);
    }
}

public class GetDreQueryValidatorTests
{
    private readonly GetDreQueryValidator _validator = new();

    [Fact]
    public void Validate_QueryValida_Passa() =>
        Assert.True(_validator.Validate(new GetDreQuery(1, 2026, 7)).IsValid);

    [Theory]
    [InlineData(0, 2026, 7)]   // filial inválida
    [InlineData(1, 1999, 7)]   // ano abaixo do intervalo
    [InlineData(1, 2101, 7)]   // ano acima do intervalo
    [InlineData(1, 2026, 0)]   // mês inválido
    [InlineData(1, 2026, 13)]  // mês inválido
    public void Validate_QueryInvalida_Falha(int cdFilial, int ano, int mes) =>
        Assert.False(_validator.Validate(new GetDreQuery(cdFilial, ano, mes)).IsValid);
}
