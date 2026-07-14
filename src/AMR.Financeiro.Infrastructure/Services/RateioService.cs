using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Executa o rateio automático mensal entre centros de custo (Card 23.5).
/// Bases suportadas: percentual fixo, área (m²) e headcount — para bases
/// dinâmicas o percentual é recalculado proporcionalmente ao ValorBase de cada destino.
/// </summary>
public class RateioService(ICentroCustoRepository repo) : IRateioService
{
    public async Task<RateioExecucaoResult> ExecutarMesAsync(int cdFilial, DateOnly competencia, CancellationToken ct = default)
    {
        if (await repo.RateioJaExecutadoAsync(cdFilial, competencia, ct))
            throw new InvalidOperationException($"Rateio {competencia:MM/yyyy} já executado.");

        var regras = await repo.GetRegrasAtivasAsync(cdFilial, ct);
        var todosRateios = new List<RateioRealizado>();
        var erros = new List<string>();
        decimal valorTotalRateado = 0;

        foreach (var regra in regras)
        {
            try
            {
                // Busca total: apenas simula um valor proporcional ao mês
                // (sem tabela de lançamentos por CC ainda — será integrado no Sprint 25)
                // Usa valor fixo de referência = 1000 para demonstrar o rateio funcional
                decimal totalConta = 1000m; // TODO: integrar com lançamentos reais via ContaOrigemDescricao

                var destinos = regra.Destinos.ToList();

                // Recalcula % para base dinâmica
                if (regra.TipoBase != TipoBaseRateio.FixoPercentual)
                {
                    decimal totalBase = destinos.Sum(d => d.ValorBase ?? 0);
                    if (totalBase > 0)
                    {
                        // Criar novos objetos percentuais calculados inline (não muta entidade)
                        var percentuaisCalculados = destinos.Select(d =>
                            (destino: d, pct: totalBase > 0 ? (d.ValorBase ?? 0) / totalBase * 100 : d.Percentual))
                            .ToList();

                        foreach (var (destino, pct) in percentuaisCalculados)
                        {
                            var valorRateado = totalConta * (pct / 100);
                            todosRateios.Add(new RateioRealizado(regra.Id, destino.CentroCustoId,
                                valorRateado, pct, competencia));
                            valorTotalRateado += valorRateado;
                        }
                        continue;
                    }
                }

                foreach (var destino in destinos)
                {
                    var valorRateado = totalConta * (destino.Percentual / 100);
                    todosRateios.Add(new RateioRealizado(regra.Id, destino.CentroCustoId,
                        valorRateado, destino.Percentual, competencia));
                    valorTotalRateado += valorRateado;
                }
            }
            catch (Exception ex)
            {
                erros.Add($"Regra '{regra.Nome}': {ex.Message}");
            }
        }

        if (todosRateios.Count > 0)
            await repo.AddRateiosAsync(todosRateios, ct);

        return new RateioExecucaoResult(regras.Count, todosRateios.Count, valorTotalRateado, erros);
    }
}
