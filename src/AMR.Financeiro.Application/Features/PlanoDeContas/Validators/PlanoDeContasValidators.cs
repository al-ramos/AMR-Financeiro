using FluentValidation;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;

namespace AMR.Financeiro.Application.Features.PlanoDeContas.Validators;

/// <summary>Validação de criação de conta do plano gerencial (Card 23.4).</summary>
public class CriarContaCommandValidator : AbstractValidator<CriarContaCommand>
{
    public CriarContaCommandValidator()
    {
        RuleFor(c => c.CdFilial)
            .GreaterThan(0).WithMessage("CdFilial deve ser maior que zero.");

        RuleFor(c => c.Codigo)
            .NotEmpty().WithMessage("Código é obrigatório.")
            .MaximumLength(20)
            .Matches(@"^\d+(\.\d+){0,4}$")
            .WithMessage("Código deve ser hierárquico no formato N[.N[.N[.N[.N]]]] — ex.: 3.1.1.1.1 (até 5 níveis).");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200);

        RuleFor(c => c.Nivel)
            .InclusiveBetween(1, 5).WithMessage("Nível deve estar entre 1 e 5.");

        RuleFor(c => c)
            .Must(c => c.Nivel == c.Codigo.Count(ch => ch == '.') + 1)
            .When(c => !string.IsNullOrWhiteSpace(c.Codigo) && c.Nivel is >= 1 and <= 5)
            .WithMessage("Nível deve corresponder à profundidade do código (ex.: 3.1.1 → nível 3).")
            .OverridePropertyName(nameof(CriarContaCommand.Nivel));

        RuleFor(c => c.PaiId)
            .NotNull()
            .When(c => c.Nivel > 1)
            .WithMessage("Contas de nível 2 a 5 devem informar a conta pai (PaiId).");

        RuleFor(c => c.PaiId)
            .Null()
            .When(c => c.Nivel == 1)
            .WithMessage("Contas de nível 1 (grupo) não devem ter conta pai.");

        RuleFor(c => c.OrdemExibicao)
            .GreaterThanOrEqualTo(0).WithMessage("Ordem de exibição não pode ser negativa.");
    }
}

/// <summary>Validação de atualização de conta do plano gerencial (Card 23.4).</summary>
public class AtualizarContaCommandValidator : AbstractValidator<AtualizarContaCommand>
{
    public AtualizarContaCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("Id deve ser maior que zero.");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200);

        RuleFor(c => c.OrdemExibicao)
            .GreaterThanOrEqualTo(0).WithMessage("Ordem de exibição não pode ser negativa.");
    }
}
