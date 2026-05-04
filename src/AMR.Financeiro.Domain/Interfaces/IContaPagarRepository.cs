using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

public interface IContaPagarRepository
{
    Task<ContaPagar?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ContaPagar>> ObterPorFilialAsync(int cdFilial, CancellationToken ct = default);
    Task AdicionarAsync(ContaPagar conta, CancellationToken ct = default);
    void Atualizar(ContaPagar conta);
}
