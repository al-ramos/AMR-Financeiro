using Moq;
using AMR.Financeiro.Application.Features.ContasReceber.Handlers;
using AMR.Financeiro.Application.Features.ContasReceber.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.ContasReceber;

public class GetContasReceberHandlerTests
{
    private readonly Mock<IContaReceberRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private GetContasReceberHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_SemContas_RetornaListaVazia()
    {
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(new List<ContaReceber>());

        var result = await CreateHandler().Handle(new GetContasReceberQuery(1), default);

        Assert.Empty(result);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ContasAbertas_RetornaMapeadasComoDto()
    {
        var contas = new List<ContaReceber>
        {
            new(1, "Cliente A", 500m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))),
            new(1, "Cliente B", 750m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), "NF-001"),
        };
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(contas);

        var result = (await CreateHandler().Handle(new GetContasReceberQuery(1), default)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Cliente A", result[0].Descricao);
        Assert.Equal(500m, result[0].Valor);
        Assert.Equal("NF-001", result[1].DocumentoOrigem);
    }

    [Fact]
    public async Task Handle_ContasVencidas_MarcaVencidasESalva()
    {
        var vencida = new ContaReceber(1, "Cliente Vencido", 200m, new DateOnly(2025, 1, 1));
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(new List<ContaReceber> { vencida });

        await CreateHandler().Handle(new GetContasReceberQuery(1), default);

        Assert.Equal(StatusContaReceber.Vencida, vencida.Status);
        _repoMock.Verify(r => r.Atualizar(It.IsAny<ContaReceber>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SemContasVencidas_NaoSalva()
    {
        var aberta = new ContaReceber(1, "Cliente Futuro", 300m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        _repoMock.Setup(r => r.ObterPorFilialAsync(1, default))
                 .ReturnsAsync(new List<ContaReceber> { aberta });

        await CreateHandler().Handle(new GetContasReceberQuery(1), default);

        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}
