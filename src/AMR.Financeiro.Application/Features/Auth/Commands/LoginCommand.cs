using MediatR;
using AMR.Financeiro.Application.Features.Auth.Dtos;

namespace AMR.Financeiro.Application.Features.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<TokenResponse?>;
