using FluentValidation;
using AMR.Financeiro.Application.Features.ContasPagar.Commands;

namespace AMR.Financeiro.Application.Features.ContasPagar.Validators;

public class CriarContaPagarValidator : AbstractValidator<CriarContaPagarCommand>
{
    public CriarContaPagarValidator()
    {
        RuleFor(x => x.CdFilial).GreaterThan(0).WithMessage("Filial inválida.");
        RuleFor(x => x.Descricao).NotEmpty().WithMessage("Descrição é obrigatória.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
        RuleFor(x => x.Vencimento).NotEqual(default(DateOnly)).WithMessage("Vencimento inválido.");
    }
}
