using MediatR;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Commands;

/// <summary>
/// Cria uma regra de rateio. <paramref name="ContaOrigemId"/> e a conta do plano cujos
/// lancamentos do mes serao rateados — sem ela o servico nao tem o que distribuir, e era
/// exatamente por isso que ele usava um valor fixo (FIN-02).
/// </summary>
public record CriarRegraRateioCommand(int CdFilial, string Nome, int ContaOrigemId,
    TipoBaseRateio TipoBase, List<RegraDestinoDto> Destinos) : IRequest<int>;

public record RegraDestinoDto(int CentroCustoId, decimal Percentual, decimal? ValorBase);

public class CriarRegraRateioHandler(ICentroCustoRepository repo, IPlanoDeContasRepository planoRepo)
    : IRequestHandler<CriarRegraRateioCommand, int>
{
    public async Task<int> Handle(CriarRegraRateioCommand cmd, CancellationToken ct)
    {
        if (cmd.Destinos is null || cmd.Destinos.Count == 0)
            throw new InvalidOperationException("A regra de rateio deve ter pelo menos um destino.");

        var somaPercentual = cmd.Destinos.Sum(d => d.Percentual);
        if (somaPercentual is < 99.99m or > 100.01m)
            throw new InvalidOperationException(
                $"A soma dos percentuais dos destinos deve ser 100% (tolerância ±0,01). Soma informada: {somaPercentual:0.##}%.");

        var conta = await planoRepo.GetByIdAsync(cmd.ContaOrigemId, ct)
            ?? throw new InvalidOperationException($"Conta de origem Id {cmd.ContaOrigemId} nao encontrada.");

        if (!conta.AceitaLancamentos)
            throw new InvalidOperationException(
                $"A conta '{conta.Codigo} - {conta.Descricao}' e sintetica e nao recebe lancamento — nao ha o que ratear a partir dela.");

        // A descricao vira rotulo, derivado da conta, em vez de texto solto digitado.
        // Usa o Id do comando — foi por ele que a conta acabou de ser buscada — em vez
        // de conta.Id, que depende da entidade vir materializada com a chave preenchida.
        var regra = new RegraRateio(
            cmd.CdFilial, cmd.Nome, cmd.ContaOrigemId, $"{conta.Codigo} - {conta.Descricao}", cmd.TipoBase);

        // RegraRateioId = 0: o EF Core preenche a FK ao salvar via navegação regra.Destinos.
        var destinos = cmd.Destinos
            .Select(d => new RegraRateioDestino(0, d.CentroCustoId, d.Percentual, d.ValorBase))
            .ToList();

        await repo.AddRegraAsync(regra, destinos, ct);
        return regra.Id;
    }
}
