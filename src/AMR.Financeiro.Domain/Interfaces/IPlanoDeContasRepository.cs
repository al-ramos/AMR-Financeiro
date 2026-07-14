using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

/// <summary>
/// Repositório do plano de contas gerencial (<see cref="PlanoDeContas"/> — Card 23.4).
/// Nomeado IPlanoDeContasRepository (e não IPlanoContasRepository) porque já existe
/// um <see cref="IPlanoContasRepository"/> legado neste namespace operando sobre
/// a entidade <see cref="PlanoContas"/>.
/// </summary>
public interface IPlanoDeContasRepository
{
    Task<List<PlanoDeContas>> GetByCdFilialAsync(int cdFilial, bool incluirInativos = false, CancellationToken ct = default);
    Task<PlanoDeContas?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PlanoDeContas?> GetByCodigoAsync(int cdFilial, string codigo, CancellationToken ct = default);
    Task<bool> TemLancamentosAsync(int contaId, CancellationToken ct = default);
    Task<bool> TemContasFilhasAsync(int contaId, CancellationToken ct = default);
    Task AddAsync(PlanoDeContas conta, CancellationToken ct = default);
    Task UpdateAsync(PlanoDeContas conta, CancellationToken ct = default);
}
