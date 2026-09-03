using FluentValidation;
using AMR.Financeiro.Application.Features.Lancamentos.Commands;

namespace AMR.Financeiro.Application.Features.Lancamentos.Validators;

public class CriarLancamentoValidator : AbstractValidator<CriarLancamentoCommand>
{
    public CriarLancamentoValidator()
    {
        RuleFor(x => x.CdFilial).GreaterThan(0).WithMessage("Filial inválida.");
        RuleFor(x => x.PlanoContasId).GreaterThan(0).WithMessage("Plano de Contas inválido.");
        RuleFor(x => x.Valor).GreaterThan(0).WithMessage("O valor deve ser maior que zero.");
        RuleFor(x => x.Historico).NotEmpty().WithMessage("Histórico é obrigatório.");
        RuleFor(x => x.DataLancamento).NotEqual(default(DateOnly)).WithMessage("Data de lançamento inválida.");
    }
}
