using FluentValidation;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;

namespace AMR.Financeiro.Application.Features.ContasReceber.Validators;

public class CriarContaReceberValidator : AbstractValidator<CriarContaReceberCommand>
{
    public CriarContaReceberValidator()
    {
        RuleFor(x => x.CdFilial).GreaterThan(0).WithMessage("Filial inválida.");
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descrição é obrigatória.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
        RuleFor(x => x.Vencimento).NotEqual(default(DateOnly)).WithMessage("Vencimento inválido.");
    }
}
