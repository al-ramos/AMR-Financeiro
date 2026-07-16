using Moq;
using AMR.Financeiro.Application.Features.CentroCusto.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Tests.Features.CentroCusto;

public class GetOrcamentoHandlerTests
{
    private readonly Mock<ICentroCustoRepository> _repoMock = new();

    private GetOrcamentoHandler CreateHandler() => new(_repoMock.Object);

    [Fact]
    public async Task Handle_SemOrcamentos_Retorna12MesesZerados()
    {
        _repoMock.Setup(r => r.GetOrcamentoAsync(5, 2026, default))
                 .ReturnsAsync([]);

        var dto = await CreateHandler().Handle(new GetOrcamentoQuery(5, 2026), default);

        Assert.Equal(12, dto.Meses.Count);
        Assert.Equal(0m, dto.TotalOrcado);
        Assert.Equal(0m, dto.TotalRealizado);
        Assert.All(dto.Meses, m =>
        {
            Assert.Equal(0m, m.Orcado);
            Assert.False(m.EmAlerta);
            Assert.False(m.Estourado);
        });
    }

    [Fact]
    public async Task Handle_AgregaContasDoMesmoMes_ECalculaComparativoOrcadoRealizado()
    {
        _repoMock.Setup(r => r.GetOrcamentoAsync(5, 2026, default))
                 .ReturnsAsync(
                 [
                     new OrcamentoCC(5, "Energia", 2026, 1, 1000m, 950m),
                     new OrcamentoCC(5, "Água", 2026, 1, 500m, 600m),
                     new OrcamentoCC(5, "Energia", 2026, 3, 200m, 0m)
                 ]);

        var dto = await CreateHandler().Handle(new GetOrcamentoQuery(5, 2026), default);

        var janeiro = dto.Meses.Single(m => m.Mes == 1);
        Assert.Equal("Janeiro", janeiro.NomeMes);
        Assert.Equal(1500m, janeiro.Orcado);   // 1000 + 500 agregados
        Assert.Equal(1550m, janeiro.Realizado); // 950 + 600 agregados
        Assert.True(janeiro.EmAlerta);          // 103,33% >= 90%
        Assert.True(janeiro.Estourado);         // 103,33% > 100%

        var marco = dto.Meses.Single(m => m.Mes == 3);
        Assert.Equal(200m, marco.Orcado);
        Assert.Equal(0m, marco.Realizado);
        Assert.False(marco.EmAlerta);

        Assert.Equal(1700m, dto.TotalOrcado);
        Assert.Equal(1550m, dto.TotalRealizado);
    }

    [Fact]
    public async Task Handle_Consumo95PorCento_MarcaEmAlertaSemEstourar()
    {
        _repoMock.Setup(r => r.GetOrcamentoAsync(5, 2026, default))
                 .ReturnsAsync([new OrcamentoCC(5, "Energia", 2026, 6, 1000m, 950m)]);

        var dto = await CreateHandler().Handle(new GetOrcamentoQuery(5, 2026), default);

        var junho = dto.Meses.Single(m => m.Mes == 6);
        Assert.Equal(95m, junho.PercentualConsumido);
        Assert.True(junho.EmAlerta);
        Assert.False(junho.Estourado);
    }
}
