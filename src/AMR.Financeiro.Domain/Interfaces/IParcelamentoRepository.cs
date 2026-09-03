using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Domain.Interfaces;

public interface IParcelamentoRepository
{
    /// <summary>Parcelamento com todas as parcelas carregadas.</summary>
    Task<Parcelamento?> ObterPorIdAsync(int id, CancellationToken ct = default);

    Task<List<Parcelamento>> ListarAsync(CancellationToken ct = default);

    /// <summary>Parcelas em aberto (Pendente/Vencido) com o parcelamento carregado — base do aging e do fluxo de caixa.</summary>
    Task<List<Parcela>> ObterParcelasEmAbertoAsync(CancellationToken ct = default);

    Task AdicionarAsync(Parcelamento parcelamento, CancellationToken ct = default);
    Task AtualizarAsync(Parcelamento parcelamento, CancellationToken ct = default);
}
