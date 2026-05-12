using MediatR;
using AMR.Financeiro.Application.Features.Auth.Commands;
using AMR.Financeiro.Application.Features.Auth.Dtos;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Shared.Security;

namespace AMR.Financeiro.Application.Features.Auth.Handlers;

public class LoginHandler(IUsuarioRepository usuarios) : IRequestHandler<LoginCommand, UsuarioValidado?>
{
    public async Task<UsuarioValidado?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.GetByUsernameAsync(request.Username, cancellationToken);
        if (usuario is null || !PasswordHelper.Verify(request.Password, usuario.PasswordHash))
            return null;

        return new UsuarioValidado(usuario.Id, usuario.Username, usuario.Role);
    }
}
