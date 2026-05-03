using Moq;
using RDS.Financeiro.Application.Features.Lancamentos.Commands;
using RDS.Financeiro.Application.Features.Lancamentos.Handlers;
using RDS.Financeiro.Domain.Entities;
using RDS.Financeiro.Domain.Enums;
using RDS.Financeiro.Domain.Interfaces;

namespace RDS.Financeiro.Tests.Features.Lancamentos;

public class CriarLancamentoHandlerTests
{
    private readonly Mock<ILancamentoFinanceiroRepository> _repoMock = new();
    private readonly Mock<IPlanoContasRepository> _planoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CriarLancamentoHandler CreateHandler() =>
        new(_repoMock.Object, _planoMock.Object, _uowMock.Object);

    private static PlanoContas PlanoAnaliticoAtivo() =>
        new(1, "1.1.1", "Caixa", TipoContaPlano.Analitica);

    private static PlanoContas PlanoSintetico() =>
        new(1, "1", "Ativo", TipoContaPlano.Sintetica);

    private static PlanoContas PlanoInativo()
    {
        var p = new PlanoContas(1, "1.1.2", "Banco Inativo", TipoContaPlano.Analitica);
        p.Inativar();
        return p;
    }

    [Fact]
    public async Task Handle_ValorZero_LancaInvalidOperationException()
    {
        var cmd = new CriarLancamentoCommand(1, 10, TipoLancamento.Credito, 0m,
            new DateOnly(2026, 5, 1), "Historico");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));
    }

    [Fact]
    public async Task Handle_ValorNegativo_LancaInvalidOperationException()
    {
        var cmd = new CriarLancamentoCommand(1, 10, TipoLancamento.Credito, -100m,
            new DateOnly(2026, 5, 1), "Historico");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));
    }

    [Fact]
    public async Task Handle_PlanoContasNaoEncontrado_LancaInvalidOperationException()
    {
        _planoMock.Setup(r => r.ObterPorIdAsync(99, default))
                  .ReturnsAsync((PlanoContas?)null);

        var cmd = new CriarLancamentoCommand(1, 99, TipoLancamento.Credito, 500m,
            new DateOnly(2026, 5, 1), "Historico");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));
    }

    [Fact]
    public async Task Handle_PlanoSintetico_LancaInvalidOperationException()
    {
        _planoMock.Setup(r => r.ObterPorIdAsync(1, default))
                  .ReturnsAsync(PlanoSintetico());

        var cmd = new CriarLancamentoCommand(1, 1, TipoLancamento.Credito, 500m,
            new DateOnly(2026, 5, 1), "Historico");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("Sintética", ex.Message);
    }

    [Fact]
    public async Task Handle_PlanoInativo_LancaInvalidOperationException()
    {
        _planoMock.Setup(r => r.ObterPorIdAsync(5, default))
                  .ReturnsAsync(PlanoInativo());

        var cmd = new CriarLancamentoCommand(1, 5, TipoLancamento.Credito, 500m,
            new DateOnly(2026, 5, 1), "Historico");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("inativa", ex.Message);
    }

    [Fact]
    public async Task Handle_DadosValidos_AdicionaLancamentoESalva()
    {
        _planoMock.Setup(r => r.ObterPorIdAsync(10, default))
                  .ReturnsAsync(PlanoAnaliticoAtivo());

        var cmd = new CriarLancamentoCommand(1, 10, TipoLancamento.Credito, 1500m,
            new DateOnly(2026, 5, 1), "Venda à vista");

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<LancamentoFinanceiro>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_OrigemEManual()
    {
        _planoMock.Setup(r => r.ObterPorIdAsync(10, default))
                  .ReturnsAsync(PlanoAnaliticoAtivo());

        LancamentoFinanceiro? capturado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<LancamentoFinanceiro>(), default))
                 .Callback<LancamentoFinanceiro, CancellationToken>((l, _) => capturado = l);

        var cmd = new CriarLancamentoCommand(1, 10, TipoLancamento.Debito, 200m,
            new DateOnly(2026, 5, 1), "Pagamento fornecedor");

        await CreateHandler().Handle(cmd, default);

        Assert.NotNull(capturado);
        Assert.Equal(OrigemLancamento.Manual, capturado!.Origem);
        Assert.Equal(TipoLancamento.Debito, capturado.Tipo);
        Assert.Equal(200m, capturado.Valor);
    }
}
