using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Executa o rateio automático mensal entre centros de custo (Card 23.5).
/// Bases suportadas: percentual fixo, área (m²) e headcount — para bases
/// dinâmicas o percentual é recalculado proporcionalmente ao ValorBase de cada destino.
///
/// O valor rateado sai dos lançamentos da conta de origem na competência. Uma regra
/// cuja conta não existe, não teve movimento, ou cuja base dinâmica está sem valor,
/// entra na lista de erros do resultado e não gera rateio — em vez de gerar zero,
/// que o centro de custo exibiria como apuração. Ver FIN-02.
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
                // O valor a ratear e o que a conta de origem acumulou na competencia.
                // Antes era `decimal totalConta = 1000m`, fixo, e o resultado era
                // persistido em RateioRealizado — alimentando DRE e orcamento com um
                // numero que ninguem apurou. Ver FIN-02.
                var total = await repo.ObterTotalDaContaAsync(cdFilial, regra.ContaOrigemId, competencia, ct);

                if (total is null)
                {
                    erros.Add($"Regra '{regra.Nome}': conta de origem {regra.ContaOrigemId} nao encontrada na filial {cdFilial}.");
                    continue;
                }

                // Sem movimento na conta nao ha o que ratear. Registrar zero seria pior
                // que nao registrar: o centro de custo passaria a exibir uma apuracao
                // que nunca aconteceu.
                if (total.Value == 0)
                {
                    erros.Add($"Regra '{regra.Nome}': a conta '{regra.ContaOrigemDescricao}' nao teve lancamento em {competencia:MM/yyyy} — nada rateado.");
                    continue;
                }

                var totalConta = total.Value;
                var destinos = regra.Destinos.ToList();

                // Recalcula % para base dinâmica
                if (regra.TipoBase != TipoBaseRateio.FixoPercentual)
                {
                    decimal totalBase = destinos.Sum(d => d.ValorBase ?? 0);

                    // Base dinamica (area, headcount) sem valor informado nao tem como
                    // ser proporcional. Cair no percentual fixo aqui seria silenciosamente
                    // usar outro criterio que o da regra.
                    if (totalBase <= 0)
                    {
                        erros.Add($"Regra '{regra.Nome}': base {regra.TipoBase} sem ValorBase nos destinos — nada rateado.");
                        continue;
                    }

                    // Percentuais calculados fora da entidade — a regra não é mutada.
                    var percentuaisCalculados = destinos
                        .Select(d => (destino: d, pct: (d.ValorBase ?? 0) / totalBase * 100))
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
