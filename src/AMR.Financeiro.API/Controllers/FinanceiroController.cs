using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.Financeiro.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/financeiro")]
public class FinanceiroController(IMediator mediator) : ControllerBase
{
    // GET api/financeiro/aging
    [HttpGet("aging")]
    public async Task<IActionResult> GetAging(CancellationToken ct) =>
        Ok(await mediator.Send(new GetAgingQuery(), ct));

    // GET api/financeiro/fluxo-caixa?horizonteDias=30
    [HttpGet("fluxo-caixa")]
    public async Task<IActionResult> GetFluxoCaixa([FromQuery] int horizonteDias = 30, CancellationToken ct = default) =>
        Ok(await mediator.Send(new GetFluxoCaixaQuery(horizonteDias), ct));
}
