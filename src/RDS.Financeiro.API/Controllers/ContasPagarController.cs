using MediatR;
using Microsoft.AspNetCore.Mvc;
using RDS.Financeiro.Application.Features.ContasPagar.Commands;
using RDS.Financeiro.Application.Features.ContasPagar.Queries;

namespace RDS.Financeiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContasPagarController(IMediator mediator) : ControllerBase
{
    // GET api/contaspagar?cdFilial=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetContasPagarQuery(cdFilial), ct);
        return Ok(result);
    }

    // GET api/contaspagar/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetContaPagarByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/contaspagar
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarContaPagarCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // PATCH api/contaspagar/5/pagar
    [HttpPatch("{id:int}/pagar")]
    public async Task<IActionResult> Pagar(int id, [FromBody] PagarContaRequest req, CancellationToken ct)
    {
        var ok = await mediator.Send(new PagarContaCommand(id, req.DataPagamento), ct);
        return ok ? NoContent() : NotFound();
    }

    // PATCH api/contaspagar/5/cancelar
    [HttpPatch("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new CancelarContaCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }
}

public record PagarContaRequest(DateOnly DataPagamento);
