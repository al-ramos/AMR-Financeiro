using FluentValidation;
using AMR.Financeiro.Application.Features.PlanoContas.Commands;

namespace AMR.Financeiro.Application.Features.PlanoContas.Validators;

public class CriarPlanoContasValidator : AbstractValidator<CriarPlanoContasCommand>
{
    public CriarPlanoContasValidator()
    {
        RuleFor(x => x.CdFilial).GreaterThan(0).WithMessage("Filial inválida.");
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("Código é obrigatório.");
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descrição é obrigatória.");
    }
}

public class AtualizarPlanoContasValidator : AbstractValidator<AtualizarPlanoContasCommand>
{
    public AtualizarPlanoContasValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descrição é obrigatória.");
    }
}
