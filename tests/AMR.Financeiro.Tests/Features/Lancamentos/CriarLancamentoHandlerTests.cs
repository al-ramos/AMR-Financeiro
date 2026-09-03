using FluentValidation;
using Moq;
using AMR.Financeiro.Application.Features.Lancamentos.Commands;
using AMR.Financeiro.Application.Features.Lancamentos.Handlers;
using AMR.Financeiro.Application.Features.Lancamentos.Validators;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.Lancamentos;

public class CriarLancamentoHandlerTests
{
    private readonly Mock<ILancamentoFinanceiroRepository> _repoMock = new();
    private readonly Mock<IPlanoContasRepository> _planoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IEventPublisher> _publisherMock = new();

    private CriarLancamentoHandler CreateHandler() =>
        new(_repoMock.Object, _planoMock.Object, _uowMock.Object, _publisherMock.Object);

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validator_ValorInvalido_RetornaErro(decimal valor)
    {
        var cmd = new CriarLancamentoCommand(1, 10, TipoLancamento.Credito, valor,
            new DateOnly(2026, 5, 1), "Historico");

        var result = new CriarLancamentoValidator().Validate(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(cmd.Valor));
    }

    [Fact]
    public async Task Handle_PlanoContasNaoEncontrado_LancaInvalidOperationException()
    {
        _planoMock.Setup(r => r.ObterPorIdAsync(99, default))
                  .ReturnsAsync((AMR.Financeiro.Domain.Entities.PlanoContas?)null);

        var cmd = new CriarLancamentoCommand(1, 99, TipoLancamento.Credito, 500m,
            new DateOnly(2026, 5, 1), "Historico");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));
    }

    [Fact]
    public async Task Handle_PlanoSintetico_LancaInvalidOperationException()
    {
        var plano = new AMR.Financeiro.Domain.Entities.PlanoContas(1, "1", "Ativo", TipoContaPlano.Sintetica);
        _planoMock.Setup(r => r.ObterPorIdAsync(1, default))
                  .ReturnsAsync(plano);

        var cmd = new CriarLancamentoCommand(1, 1, TipoLancamento.Credito, 500m,
            new DateOnly(2026, 5, 1), "Historico");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("Sintética", ex.Message);
    }

    [Fact]
    public async Task Handle_PlanoInativo_LancaInvalidOperationException()
    {
        var plano = new AMR.Financeiro.Domain.Entities.PlanoContas(1, "1.1.2", "Banco Inativo", TipoContaPlano.Analitica);
        plano.Inativar();
        _planoMock.Setup(r => r.ObterPorIdAsync(5, default))
                  .ReturnsAsync(plano);

        var cmd = new CriarLancamentoCommand(1, 5, TipoLancamento.Credito, 500m,
            new DateOnly(2026, 5, 1), "Historico");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(cmd, default));

        Assert.Contains("inativa", ex.Message);
    }

    [Fact]
    public async Task Handle_DadosValidos_AdicionaLancamentoESalva()
    {
        var plano = new AMR.Financeiro.Domain.Entities.PlanoContas(1, "1.1.1", "Caixa", TipoContaPlano.Analitica);
        _planoMock.Setup(r => r.ObterPorIdAsync(10, default))
                  .ReturnsAsync(plano);

        var cmd = new CriarLancamentoCommand(1, 10, TipoLancamento.Credito, 1500m,
            new DateOnly(2026, 5, 1), "Venda à vista");

        await CreateHandler().Handle(cmd, default);

        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<LancamentoFinanceiro>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DadosValidos_OrigemEManual()
    {
        var plano = new AMR.Financeiro.Domain.Entities.PlanoContas(1, "1.1.1", "Caixa", TipoContaPlano.Analitica);
        _planoMock.Setup(r => r.ObterPorIdAsync(10, default))
                  .ReturnsAsync(plano);

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
