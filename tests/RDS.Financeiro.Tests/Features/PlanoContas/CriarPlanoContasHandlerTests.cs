using Moq;
using RDS.Financeiro.Application.Features.PlanoContas.Commands;
using RDS.Financeiro.Application.Features.PlanoContas.Handlers;
using RDS.Financeiro.Domain.Enums;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Tests.Features.PlanoContas;

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
}
