using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using AMR.Financeiro.Application.Common.Behaviors;

namespace AMR.Financeiro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Executa os validators FluentValidation no pipeline do MediatR.
            // Registrado uma única vez: main e develop haviam adicionado o mesmo
            // behavior por caminhos diferentes (cfg.AddBehavior e AddTransient),
            // o que faria cada request ser validado duas vezes.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
