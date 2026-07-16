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
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Executa os validators FluentValidation no pipeline do MediatR (Cards 23.4/23.5)
        // Requests sem validator passam direto — neutro para as demais features.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
