using AMR.Financeiro.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AMR.Financeiro.API.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.Sucesso)
            return controller.Ok(result.Valor);

        if (result.Erro.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
            return controller.Problem(
                detail: result.Erro,
                statusCode: StatusCodes.Status404NotFound,
                title: "Não encontrado");

        return controller.Problem(
            detail: result.Erro,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Requisição inválida");
    }
}
