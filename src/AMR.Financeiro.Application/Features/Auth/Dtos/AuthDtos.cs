namespace AMR.Financeiro.Application.Features.Auth.Dtos;

public record LoginRequest(string Username, string Password);

public record TokenResponse(string Token, string Username, string Role, DateTime ExpiresAt);
