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
        // SQL direto: EF Core 9 + SQLite não gera corretamente o INSERT omitindo
        // a PK auto-gerada (NOT NULL constraint failed). Workaround definitivo.
        var now = usuario.CriadoEm.ToString("o");
        await db.Database.ExecuteSqlRawAsync($@"
            INSERT INTO ""Usuarios"" (""Username"", ""PasswordHash"", ""Role"", ""CriadoEm"")
            VALUES ('{usuario.Username.Replace("'", "''")}',
                    '{usuario.PasswordHash.Replace("'", "''")}',
                    '{usuario.Role.Replace("'", "''")}',
                    '{now}')
        ", ct);
    }
}
