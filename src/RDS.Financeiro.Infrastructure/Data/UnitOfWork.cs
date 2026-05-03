using RDS.Financeiro.Domain.Interfaces;
using RDS.Financeiro.Infrastructure.Data;

namespace RDS.Financeiro.Infrastructure.Data;

public class UnitOfWork(FinanceiroDbContext ctx) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        ctx.SaveChangesAsync(ct);
}
