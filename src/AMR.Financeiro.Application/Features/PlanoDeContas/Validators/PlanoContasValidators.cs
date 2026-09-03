using FluentValidation;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Validators;

// Vindos de main, onde ainda se chamavam CriarPlanoContasCommand e
// AtualizarPlanoContasCommand. A consolidacao em develop renomeou os commands
// para CriarContaCommand e AtualizarContaCommand — as regras sao as mesmas.
public class CriarContaValidator : AbstractValidator<CriarContaCommand>
{
    public CriarContaValidator()
    {
        RuleFor(x => x.CdFilial).GreaterThan(0).WithMessage("Filial inválida.");
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("Código é obrigatório.");
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descrição é obrigatória.");
    }
}

public class AtualizarContaValidator : AbstractValidator<AtualizarContaCommand>
{
    public AtualizarContaValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descrição é obrigatória.");
    }
}
