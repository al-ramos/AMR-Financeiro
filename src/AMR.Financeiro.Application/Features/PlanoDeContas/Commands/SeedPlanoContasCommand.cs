using MediatR;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

/// <summary>Aplica o plano de contas padrão (CFC) para a filial. Uso em desenvolvimento/setup inicial.</summary>
public record SeedPlanoContasCommand(int CdFilial) : IRequest<int>; // retorna qtde de contas criadas

public class SeedPlanoContasHandler(IPlanoContasSeedService seedService)
    : IRequestHandler<SeedPlanoContasCommand, int>
{
    public Task<int> Handle(SeedPlanoContasCommand cmd, CancellationToken ct) =>
        seedService.SeedAsync(cmd.CdFilial, ct);
}
