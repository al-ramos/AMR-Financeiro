using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Interfaces;

public interface INFeRepository
{
    Task<NotaFiscal?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<NotaFiscal?> GetByChaveAcessoAsync(string chaveAcesso, CancellationToken ct = default);
    Task<IReadOnlyList<NotaFiscal>> GetByCdFilialAsync(int cdFilial, int? ano = null, CancellationToken ct = default);
    Task AddAsync(NotaFiscal nfe, CancellationToken ct = default);
    Task UpdateAsync(NotaFiscal nfe, CancellationToken ct = default);
    Task<long> GetNextNumeroNFAsync(int cdFilial, ModeloNFe modelo, int serie, CancellationToken ct = default);
}
