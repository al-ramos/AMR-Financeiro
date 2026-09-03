using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.Lancamentos.Commands;
using AMR.Financeiro.Application.Features.Lancamentos.Queries;
using AMR.Financeiro.API.Helpers;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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

    // GET api/lancamentos/export?cdFilial=1&inicio=2026-01-01&fim=2026-12-31
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] int cdFilial,
        [FromQuery] DateOnly? inicio,
        [FromQuery] DateOnly? fim,
        CancellationToken ct)
    {
        IEnumerable<Application.Features.Lancamentos.Dtos.LancamentoFinanceiroDto> result;
        if (inicio.HasValue && fim.HasValue)
            result = await mediator.Send(new GetLancamentosByPeriodoQuery(cdFilial, inicio.Value, fim.Value), ct);
        else
            result = await mediator.Send(new GetLancamentosQuery(cdFilial), ct);

        return CsvExportHelper.Export(result, "lancamentos");
    }

    // POST api/lancamentos
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CriarLancamentoCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
