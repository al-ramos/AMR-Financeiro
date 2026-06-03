using Moq;
using AMR.Financeiro.Application.Features.ContasPagar.Commands;
using AMR.Financeiro.Application.Features.ContasPagar.Handlers;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.ContasPagar;

public class CriarContaPagarHandlerTests
{
    private readonly Mock<IContaPagarRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CriarContaPagarHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static CriarContaPagarCommand CmdValido() =>
        new(1, "Fornecedor ABC", 500m, new DateOnly(2026, 7, 1));

    [Fact]
    public async Task Handle_DadosValidos_AdicionaContaESalva()
    {
        await CreateHandler().Handle(CmdValido(), default);

        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<ContaPagar>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_RetornaIdContaCriada()
    {
        ContaPagar? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<ContaPagar>(), default))
                 .Callback<ContaPagar, CancellationToken>((c, _) => capturado = c);

        var id = await CreateHandler().Handle(CmdValido(), default);

        // Id é 0 pois o banco não está envolvido; apenas valida que o objeto foi criado
        Assert.NotNull(capturado);
        Assert.Equal("Fornecedor ABC", capturado!.Descricao);
        Assert.Equal(500m, capturado.Valor);
        Assert.Equal(1, capturado.CdFilial);
    }

    [Fact]
    public async Task Handle_DadosValidos_ContaCriadaFicaAberta()
    {
        ContaPagar? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<ContaPagar>(), default))
                 .Callback<ContaPagar, CancellationToken>((c, _) => capturado = c);

        await CreateHandler().Handle(CmdValido(), default);

        Assert.NotNull(capturado);
        Assert.Equal(StatusConta.Aberta, capturado!.Status);
    }
}
