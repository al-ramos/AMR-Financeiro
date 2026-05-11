using MediatR;
using AMR.Financeiro.Application.Features.ContasReceber.Dtos;
using AMR.Financeiro.Application.Features.ContasReceber.Queries;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasReceber.Handlers;

public class GetContasReceberHandler(IContaReceberRepository repo, IUnitOfWork uow)
    : IRequestHandler<GetContasReceberQuery, IEnumerable<ContaReceberDto>>
{
    public async Task<IEnumerable<ContaReceberDto>> Handle(GetContasReceberQuery q, CancellationToken ct)
    {
        var contas = await repo.ObterPorFilialAsync(q.CdFilial, ct);
        var lista = contas.ToList();

        // Auto-marcar vencidas
        bool houveAlteracao = false;
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var c in lista.Where(c => c.Status == StatusContaReceber.Aberta && c.Vencimento < hoje))
        {
            c.MarcarVencida();
            repo.Atualizar(c);
            houveAlteracao = true;
        }
        if (houveAlteracao)
            await uow.SaveChangesAsync(ct);

        return lista.Select(c => new ContaReceberDto(
            c.Id, c.CdFilial, c.Descricao, c.Valor,
            c.Vencimento, c.DataRecebimento, c.ValorRecebido,
            c.DocumentoOrigem, c.Status.ToString()));
    }
}
