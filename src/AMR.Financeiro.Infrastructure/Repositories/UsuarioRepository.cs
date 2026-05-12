using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Repositories;

public class UsuarioRepository(FinanceiroDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Username == username.ToLower().Trim(), ct);

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
    }
}
