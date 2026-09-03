using MediatR;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.CentroCusto.Commands;

public record ExecutarRateioCommand(int CdFilial, int Ano, int Mes) : IRequest<RateioExecucaoResult>;

public class ExecutarRateioHandler(IRateioService rateioService)
    : IRequestHandler<ExecutarRateioCommand, RateioExecucaoResult>
{
    public Task<RateioExecucaoResult> Handle(ExecutarRateioCommand cmd, CancellationToken ct) =>
        rateioService.ExecutarMesAsync(cmd.CdFilial, new DateOnly(cmd.Ano, cmd.Mes, 1), ct);
}
