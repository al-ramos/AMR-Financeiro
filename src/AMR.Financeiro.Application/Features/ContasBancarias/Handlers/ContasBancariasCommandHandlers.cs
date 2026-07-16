using MediatR;
using AMR.Financeiro.Application.Features.ContasBancarias.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasBancarias.Handlers;

public class CriarContaBancariaHandler(IContaBancariaRepository repo, IUnitOfWork uow)
    : IRequestHandler<CriarContaBancariaCommand, int>
{
    public async Task<int> Handle(CriarContaBancariaCommand req, CancellationToken ct)
    {
        var conta = new ContaBancaria(
            req.Nome, req.Banco, req.Agencia, req.Conta,
            req.TipoConta, req.SaldoInicial, req.DataSaldoInicial);

        await repo.AdicionarAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return conta.Id;
    }
}

public class AtualizarContaBancariaHandler(IContaBancariaRepository repo, IUnitOfWork uow)
    : IRequestHandler<AtualizarContaBancariaCommand, bool>
{
    public async Task<bool> Handle(AtualizarContaBancariaCommand req, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(req.Id, ct);
        if (conta is null) return false;

        conta.Atualizar(req.Nome, req.Banco, req.Agencia, req.Conta,
            req.TipoConta, req.SaldoInicial, req.DataSaldoInicial);

        await repo.AtualizarAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public class DesativarContaBancariaHandler(IContaBancariaRepository repo, IUnitOfWork uow)
    : IRequestHandler<DesativarContaBancariaCommand, bool>
{
    public async Task<bool> Handle(DesativarContaBancariaCommand req, CancellationToken ct)
    {
        var conta = await repo.ObterPorIdAsync(req.Id, ct);
        if (conta is null) return false;

        conta.Desativar();
        await repo.AtualizarAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
