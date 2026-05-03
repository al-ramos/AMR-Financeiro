using RDS.Financeiro.Domain.Entities;

namespace RDS.Financeiro.Domain.Interfaces;

public interface IPlanoContasRepository
{
    Task<PlanoContas?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<PlanoContas?> ObterPorCodigoAsync(int cdFilial, string codigo, CancellationToken ct = default);
    Task<IEnumerable<PlanoContas>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default);
    Task<IEnumerable<PlanoContas>> ObterFilhosAsync(int paiId, CancellationToken ct = default);
    Task AdicionarAsync(PlanoContas conta, CancellationToken ct = default);
    void Atualizar(PlanoContas conta);
}
