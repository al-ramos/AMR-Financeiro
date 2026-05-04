using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

public interface IContaReceberRepository
{
    Task<ContaReceber?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ContaReceber>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default);
    Task<IEnumerable<ContaReceber>> ObterVencidasAsync(int cdFilial, CancellationToken ct = default);
    Task AdicionarAsync(ContaReceber conta, CancellationToken ct = default);
    void Atualizar(ContaReceber conta);
}
