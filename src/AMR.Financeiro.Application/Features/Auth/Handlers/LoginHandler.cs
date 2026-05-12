using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AMR.Financeiro.Application.Features.Auth.Commands;
using AMR.Financeiro.Application.Features.Auth.Dtos;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Shared.Security;

namespace AMR.Financeiro.Application.Features.Auth.Handlers;

public class LoginHandler(IUsuarioRepository usuarios, IConfiguration config) : IRequestHandler<LoginCommand, TokenResponse?>
{
    public async Task<TokenResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.GetByUsernameAsync(request.Username, cancellationToken);
        if (usuario is null || !PasswordHelper.Verify(request.Password, usuario.PasswordHash))
            return null;

        var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
        var jwtIssuer = config["Jwt:Issuer"] ?? "AMR.Financeiro";
        var jwtAudience = config["Jwt:Audience"] ?? "AMR";
        var expiresInHours = int.Parse(config["Jwt:ExpiresInHours"] ?? "8");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(expiresInHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new TokenResponse(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Username: usuario.Username,
            Role: usuario.Role,
            ExpiresAt: expiresAt);
    }
}
