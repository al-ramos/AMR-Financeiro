using Moq;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Application.Features.ContasReceber.Handlers;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.ContasReceber;

public class CriarContaReceberHandlerTests
{
    private readonly Mock<IContaReceberRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CriarContaReceberHandler CreateHandler() =>
        new(_repoMock.Object, _uowMock.Object);

    private static CriarContaReceberCommand CmdValido(string? docOrigem = null) =>
        new(1, "Cliente ABC", 800m, new DateOnly(2026, 7, 15), docOrigem);

    [Fact]
    public async Task Handle_DadosValidos_AdicionaContaESalva()
    {
        await CreateHandler().Handle(CmdValido(), default);

        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<ContaReceber>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_ContaCriadaFicaAberta()
    {
        ContaReceber? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<ContaReceber>(), default))
                 .Callback<ContaReceber, CancellationToken>((c, _) => capturado = c);

        await CreateHandler().Handle(CmdValido(), default);

        Assert.NotNull(capturado);
        Assert.Equal(StatusContaReceber.Aberta, capturado!.Status);
        Assert.Equal("Cliente ABC", capturado.Descricao);
        Assert.Equal(800m, capturado.Valor);
    }

    [Fact]
    public async Task Handle_ComDocumentoOrigem_PreservadoNaConta()
    {
        ContaReceber? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<ContaReceber>(), default))
                 .Callback<ContaReceber, CancellationToken>((c, _) => capturado = c);

        await CreateHandler().Handle(CmdValido("NF-1234"), default);

        Assert.NotNull(capturado);
        Assert.Equal("NF-1234", capturado!.DocumentoOrigem);
    }
}
