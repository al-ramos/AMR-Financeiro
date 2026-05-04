using MediatR;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.PlanoContas.Commands;

public record CriarPlanoContasCommand(
    int CdFilial,
    string Codigo,
    string Descricao,
    TipoContaPlano Tipo,
    int? PaiId
) : IRequest<int>;

public record AtualizarPlanoContasCommand(
    int Id,
    string Descricao
) : IRequest<bool>;

public record InativarPlanoContasCommand(int Id) : IRequest<bool>;

public record AtivarPlanoContasCommand(int Id) : IRequest<bool>;
