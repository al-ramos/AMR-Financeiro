using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Interfaces;

public interface IBoletoRepository
{
    Task<Boleto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Boleto>> GetByCdFilialAsync(int cdFilial, StatusBoleto? status, BancoBoleto? banco, CancellationToken ct = default);
    Task<Boleto?> GetByNossoNumeroAsync(string nossoNumero, BancoBoleto banco, CancellationToken ct = default);
    Task<int> GetNextNossoNumeroAsync(BancoBoleto banco, int cdFilial, CancellationToken ct = default);
    Task AddAsync(Boleto boleto, CancellationToken ct = default);
    Task UpdateAsync(Boleto boleto, CancellationToken ct = default);
    Task AddRemessaAsync(RemessaBancaria remessa, CancellationToken ct = default);
    Task AddRetornoAsync(RetornoBancario retorno, CancellationToken ct = default);
}
