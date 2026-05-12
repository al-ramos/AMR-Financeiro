namespace AMR.Financeiro.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = "User";
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Usuario() { }

    public static Usuario Criar(string username, string passwordHash, string role = "User")
        => new() { Username = username.ToLower().Trim(), PasswordHash = passwordHash, Role = role };
}
