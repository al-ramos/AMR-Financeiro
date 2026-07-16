using FluentValidation;
using AMR.Financeiro.Application.Features.DRE.Queries;

namespace AMR.Financeiro.Application.Features.DRE.Validators;

/// <summary>Regras comuns de período da DRE (Card 23.4).</summary>
internal static class DreRules
{
    public static IRuleBuilderOptions<T, int> CdFilialValida<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0).WithMessage("CdFilial deve ser maior que zero.");

    public static IRuleBuilderOptions<T, int> AnoValido<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(2000, 2100).WithMessage("Ano deve estar entre 2000 e 2100.");

    public static IRuleBuilderOptions<T, int> MesValido<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12.");
}

public class GetDreQueryValidator : AbstractValidator<GetDreQuery>
{
    public GetDreQueryValidator()
    {
        RuleFor(q => q.CdFilial).CdFilialValida();
        RuleFor(q => q.Ano).AnoValido();
        RuleFor(q => q.Mes).MesValido();
    }
}

public class ExportDreExcelQueryValidator : AbstractValidator<ExportDreExcelQuery>
{
    public ExportDreExcelQueryValidator()
    {
        RuleFor(q => q.CdFilial).CdFilialValida();
        RuleFor(q => q.Ano).AnoValido();
        RuleFor(q => q.Mes).MesValido();
    }
}

public class ExportDrePdfQueryValidator : AbstractValidator<ExportDrePdfQuery>
{
    public ExportDrePdfQueryValidator()
    {
        RuleFor(q => q.CdFilial).CdFilialValida();
        RuleFor(q => q.Ano).AnoValido();
        RuleFor(q => q.Mes).MesValido();
    }
}
