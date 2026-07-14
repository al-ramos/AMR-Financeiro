using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

public interface IConciliacaoRepository
{
    Task AddExtratoAsync(ExtratoBancario extrato, CancellationToken ct = default);
    Task<ExtratoBancario?> GetExtratoByIdAsync(int extratoId, CancellationToken ct = default);
    Task<List<MovimentacaoBancaria>> GetMovimentacoesByExtratoAsync(int extratoId, CancellationToken ct = default);
    Task<MovimentacaoBancaria?> GetMovimentacaoByIdAsync(int id, CancellationToken ct = default);
    Task AddMovimentacaoAsync(MovimentacaoBancaria mov, CancellationToken ct = default);
    Task UpdateMovimentacaoAsync(MovimentacaoBancaria mov, CancellationToken ct = default);
    Task<List<MovimentacaoBancaria>> GetPendentesAsync(int cdFilial, DateOnly dataMinima, CancellationToken ct = default);
    Task<bool> ExtratoJaImportadoAsync(int cdFilial, string hashArquivo, CancellationToken ct = default);
    Task SalvarHashAsync(int cdFilial, string hashArquivo, int extratoId, CancellationToken ct = default);
}
