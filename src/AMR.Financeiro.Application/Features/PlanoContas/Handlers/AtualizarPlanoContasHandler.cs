using MediatR;
using AMR.Financeiro.Application.Features.PlanoContas.Commands;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.PlanoContas.Handlers;

public class AtualizarPlanoContasHandler(IPlanoContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<AtualizarPlanoContasCommand, bool>
{
    public async Task<bool> Handle(AtualizarPlanoContasCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Atualizar(cmd.Descricao);
        repo.Atualizar(conta);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public class InativarPlanoContasHandler(IPlanoContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<InativarPlanoContasCommand, bool>
{
    public async Task<bool> Handle(InativarPlanoContasCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        // Valida se tem filhos ativos
        var filhos = await repo.ObterFilhosAsync(cmd.Id, ct);
        if (filhos.Any(f => f.Ativo))
            throw new InvalidOperationException("Não é possível inativar uma conta que possui contas filhas ativas.");

        conta.Inativar();
        repo.Atualizar(conta);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public class AtivarPlanoContasHandler(IPlanoContasRepository repo, IUnitOfWork uow)
    : IRequestHandler<AtivarPlanoContasCommand, bool>
{
    public async Task<bool> Handle(AtivarPlanoContasCommand cmd, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(cmd.Id, ct);
        if (conta is null) return false;

        conta.Ativar();
        repo.Atualizar(conta);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
