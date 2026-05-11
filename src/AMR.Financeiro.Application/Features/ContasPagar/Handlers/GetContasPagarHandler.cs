using MediatR;
using AMR.Financeiro.Application.Features.ContasPagar.Dtos;
using AMR.Financeiro.Application.Features.ContasPagar.Queries;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasPagar.Handlers;

public class GetContasPagarHandler(IContaPagarRepository repo, IUnitOfWork uow)
    : IRequestHandler<GetContasPagarQuery, IEnumerable<ContaPagarDto>>
{
    public async Task<IEnumerable<ContaPagarDto>> Handle(GetContasPagarQuery q, CancellationToken ct)
    {
        var contas = await repo.ObterPorFilialAsync(q.CdFilial, ct);
        var lista = contas.ToList();

        // Auto-marcar vencidas (apenas as que mudaram de status)
        bool houveAlteracao = false;
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var c in lista.Where(c => c.Status == StatusConta.Aberta && c.Vencimento < hoje))
        {
            c.MarcarVencida();
            repo.Atualizar(c);
            houveAlteracao = true;
        }
        if (houveAlteracao)
            await uow.SaveChangesAsync(ct);

        return lista.Select(c => new ContaPagarDto(
            c.Id, c.CdFilial, c.Descricao, c.Valor,
            c.Vencimento, c.DataPagamento, c.Status.ToString()));
    }
}
