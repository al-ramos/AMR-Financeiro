using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.Lancamentos.Commands;
using AMR.Financeiro.Application.Features.Lancamentos.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LancamentosController(IMediator mediator) : ControllerBase
{
    // GET api/lancamentos?cdFilial=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLancamentosQuery(cdFilial), ct);
        return Ok(result);
    }

    // GET api/lancamentos/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLancamentoByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // GET api/lancamentos/periodo?cdFilial=1&inicio=2026-01-01&fim=2026-05-31
    [HttpGet("periodo")]
    public async Task<IActionResult> GetByPeriodo(
        [FromQuery] int cdFilial,
        [FromQuery] DateOnly inicio,
        [FromQuery] DateOnly fim,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetLancamentosByPeriodoQuery(cdFilial, inicio, fim), ct);
        return Ok(result);
    }

    // GET api/lancamentos/planocontas/14
    [HttpGet("planocontas/{planoContasId:int}")]
    public async Task<IActionResult> GetByPlanoContas(int planoContasId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLancamentosByPlanoContasQuery(planoContasId), ct);
        return Ok(result);
    }

    // POST api/lancamentos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarLancamentoCommand cmd, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }
}
