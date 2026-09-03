using FluentValidation;
using AMR.Financeiro.Application.Features.CentroCusto.Commands;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Application.Features.CentroCusto.Validators;

/// <summary>Validação de criação de centro de custo (Card 23.5).</summary>
public class CriarCentroCustoCommandValidator : AbstractValidator<CriarCentroCustoCommand>
{
    public CriarCentroCustoCommandValidator()
    {
        RuleFor(c => c.CdFilial)
            .GreaterThan(0).WithMessage("CdFilial deve ser maior que zero.");

        RuleFor(c => c.Codigo)
            .NotEmpty().WithMessage("Código é obrigatório.")
            .MaximumLength(20)
            .Matches(@"^\d+(\.\d+){0,2}$")
            .WithMessage("Código deve ser hierárquico no formato N[.N[.N]] — ex.: 1.2.3 (até 3 níveis).");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200);

        RuleFor(c => c.Nivel)
            .InclusiveBetween(1, 3).WithMessage("Nível deve estar entre 1 e 3.");

        RuleFor(c => c)
            .Must(c => c.Nivel == c.Codigo.Count(ch => ch == '.') + 1)
            .When(c => !string.IsNullOrWhiteSpace(c.Codigo) && c.Nivel is >= 1 and <= 3)
            .WithMessage("Nível deve corresponder à profundidade do código (ex.: 1.2.3 → nível 3).");

        RuleFor(c => c.ResponsavelNome)
            .NotEmpty().WithMessage("Responsável é obrigatório.")
            .MaximumLength(200);

        RuleFor(c => c.PaiId)
            .GreaterThan(0).When(c => c.PaiId.HasValue)
            .WithMessage("PaiId, quando informado, deve ser maior que zero.");

        RuleFor(c => c)
            .Must(c => c.Nivel > 1 || c.PaiId is null)
            .WithMessage("Centro de custo de nível 1 (grupo) não pode ter pai.");

        RuleFor(c => c)
            .Must(c => c.Nivel == 1 || c.PaiId is not null)
            .WithMessage("Centro de custo de nível 2 ou 3 deve informar o centro de custo pai.");
    }
}

/// <summary>Validação de criação de regra de rateio (Card 23.5).</summary>
public class CriarRegraRateioCommandValidator : AbstractValidator<CriarRegraRateioCommand>
{
    public CriarRegraRateioCommandValidator()
    {
        RuleFor(c => c.CdFilial)
            .GreaterThan(0).WithMessage("CdFilial deve ser maior que zero.");

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome da regra é obrigatório.")
            .MaximumLength(150);

        RuleFor(c => c.ContaOrigemDescricao)
            .NotEmpty().WithMessage("Conta de origem é obrigatória.")
            .MaximumLength(200);

        RuleFor(c => c.Destinos)
            .NotEmpty().WithMessage("A regra de rateio deve ter pelo menos um destino.");

        RuleForEach(c => c.Destinos).ChildRules(d =>
        {
            d.RuleFor(x => x.CentroCustoId)
                .GreaterThan(0).WithMessage("CentroCustoId do destino deve ser maior que zero.");

            d.RuleFor(x => x.Percentual)
                .InclusiveBetween(0m, 100m).WithMessage("Percentual do destino deve estar entre 0 e 100.");
        });

        RuleFor(c => c.Destinos)
            .Must(destinos =>
            {
                var soma = destinos.Sum(d => d.Percentual);
                return soma is >= 99.99m and <= 100.01m;
            })
            .When(c => c.Destinos is { Count: > 0 })
            .WithMessage("A soma dos percentuais dos destinos deve ser 100% (tolerância ±0,01).");

        RuleForEach(c => c.Destinos)
            .Must(d => d.ValorBase is > 0m)
            .When(c => c.TipoBase != TipoBaseRateio.FixoPercentual)
            .WithMessage("ValorBase (m² ou headcount) deve ser informado e maior que zero para bases dinâmicas.");
    }
}

/// <summary>Validação de atualização de orçamento por centro de custo (Card 23.5).</summary>
public class AtualizarOrcamentoCommandValidator : AbstractValidator<AtualizarOrcamentoCommand>
{
    public AtualizarOrcamentoCommandValidator()
    {
        RuleFor(c => c.CentroCustoId)
            .GreaterThan(0).WithMessage("CentroCustoId deve ser maior que zero.");

        RuleFor(c => c.ContaDescricao)
            .NotEmpty().WithMessage("Conta é obrigatória.")
            .MaximumLength(200);

        RuleFor(c => c.Ano)
            .InclusiveBetween(2000, 2100).WithMessage("Ano deve estar entre 2000 e 2100.");

        RuleFor(c => c.Mes)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12.");

        RuleFor(c => c.ValorOrcado)
            .GreaterThanOrEqualTo(0m).WithMessage("Valor orçado não pode ser negativo.");
    }
}

/// <summary>Validação de execução do rateio mensal (Card 23.5).</summary>
public class ExecutarRateioCommandValidator : AbstractValidator<ExecutarRateioCommand>
{
    public ExecutarRateioCommandValidator()
    {
        RuleFor(c => c.CdFilial)
            .GreaterThan(0).WithMessage("CdFilial deve ser maior que zero.");

        RuleFor(c => c.Ano)
            .InclusiveBetween(2000, 2100).WithMessage("Ano deve estar entre 2000 e 2100.");

        RuleFor(c => c.Mes)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12.");
    }
}
