using Moq;
using AMR.Financeiro.Application.Features.ContasPagar.Handlers;
using AMR.Financeiro.Application.Features.ContasPagar.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.ContasPagar;

public class GetContasPagarHandlerTests
{
    private readonly Mock<IContaPagarRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private GetContasPagarHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_SemContas_RetornaListaVazia()
    {
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(new List<ContaPagar>());

        var result = await CreateHandler().Handle(new GetContasPagarQuery(1), default);

        Assert.Empty(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContasAbertas_RetornaMapeadasComoDto()
    {
        var contas = new List<ContaPagar>
        {
            new(1, "Fornecedor A", 100m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))),
            new(1, "Fornecedor B", 200m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20))),
        };
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(contas);

        var result = (await CreateHandler().Handle(new GetContasPagarQuery(1), default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Fornecedor A", result[0].Descricao);
        Assert.Equal(100m, result[0].Valor);
    }

    [Fact]
    public async Task Handle_ContasVencidas_MarcaVencidasESalva()
    {
        var vencida = new ContaPagar(1, "Conta Vencida", 150m, new DateOnly(2025, 1, 1));
        // Status inicial é Aberta; data é no passado → deve ser marcada como Vencida
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(new List<ContaPagar> { vencida });

        await CreateHandler().Handle(new GetContasPagarQuery(1), default);

        Assert.Equal(StatusConta.Vencida, vencida.Status);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaPagar>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SemContasVencidas_NaoSalva()
    {
        var aberta = new ContaPagar(1, "Conta Futura", 100m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(new List<ContaPagar> { aberta });

        await CreateHandler().Handle(new GetContasPagarQuery(1), default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}
