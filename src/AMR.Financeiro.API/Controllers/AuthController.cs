using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using AMR.Financeiro.Application.Features.Auth.Commands;
using AMR.Financeiro.Application.Features.Auth.Dtos;

namespace AMR.Financeiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, IConfiguration config) : ControllerBase
{
    /// <summary>
    /// Autentica o usuário e retorna um token JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var usuario = await mediator.Send(new LoginCommand(request.Username, request.Password), ct);
        if (usuario is null)
            return Unauthorized(new { message = "Usuário ou senha inválidos." });

        var token = GerarJwt(usuario);
        return Ok(token);
    }

    private TokenResponse GerarJwt(UsuarioValidado usuario)
    {
        var jwtKey     = config["Jwt:Key"]           ?? throw new InvalidOperationException("Jwt:Key não configurado");
        var jwtIssuer  = config["Jwt:Issuer"]        ?? "AMR.Financeiro";
        var jwtAudience = config["Jwt:Audience"]     ?? "AMR";
        var expiresInHours = int.Parse(config["Jwt:ExpiresInHours"] ?? "8");

        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(expiresInHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username),
            new Claim(ClaimTypes.Role,                    usuario.Role),
            new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:            jwtIssuer,
            audience:          jwtAudience,
            claims:            claims,
            expires:           expiry,
            signingCredentials: creds);

        return new TokenResponse(
            Token:     new JwtSecurityTokenHandler().WriteToken(token),
            Username:  usuario.Username,
            Role:      usuario.Role,
            ExpiresAt: expiry);
    }
}
