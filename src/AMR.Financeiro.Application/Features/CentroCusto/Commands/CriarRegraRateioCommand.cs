using MediatR;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Commands;

public record CriarRegraRateioCommand(int CdFilial, string Nome, string ContaOrigemDescricao,
    TipoBaseRateio TipoBase, List<RegraDestinoDto> Destinos) : IRequest<int>;

public record RegraDestinoDto(int CentroCustoId, decimal Percentual, decimal? ValorBase);

public class CriarRegraRateioHandler(ICentroCustoRepository repo)
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

        var regra = new RegraRateio(cmd.CdFilial, cmd.Nome, cmd.ContaOrigemDescricao, cmd.TipoBase);

        // RegraRateioId = 0: o EF Core preenche a FK ao salvar via navegação regra.Destinos.
        var destinos = cmd.Destinos
            .Select(d => new RegraRateioDestino(0, d.CentroCustoId, d.Percentual, d.ValorBase))
            .ToList();

        await repo.AddRegraAsync(regra, destinos, ct);
        return regra.Id;
    }
}
