namespace AMR.Financeiro.Application.Features.Auth.Dtos;

public record LoginRequest(string Username, string Password);

// Retornado pelo LoginHandler — sem dependência de JWT
public record UsuarioValidado(int Id, string Username, string Role);

// Retornado pelo AuthController ao frontend
public record TokenResponse(string Token, string Username, string Role, DateTime ExpiresAt);
