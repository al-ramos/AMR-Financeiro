using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.ContasReceber.Commands;
using AMR.Financeiro.Application.Features.ContasReceber.Queries;
using AMR.Financeiro.API.Helpers;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ContasReceberController(IMediator mediator) : ControllerBase
{
    // GET api/contasreceber?cdFilial=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetContasReceberQuery(cdFilial), ct);
        return Ok(result);
    }

    // GET api/contasreceber/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetContaReceberByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/contasreceber
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarContaReceberCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // PATCH api/contasreceber/5/receber
    [HttpPatch("{id:int}/receber")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Receber(int id, [FromBody] ReceberContaRequest req, CancellationToken ct)
    {
        var ok = await mediator.Send(new ReceberContaCommand(id, req.DataRecebimento, req.ValorRecebido), ct);
        return ok ? NoContent() : NotFound();
    }

    // PATCH api/contasreceber/5/cancelar
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new CancelarContaReceberCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }

    // GET api/contasreceber/export?cdFilial=1
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetContasReceberQuery(cdFilial), ct);
        return CsvExportHelper.Export(result, "contas_receber");
    }
}

public record ReceberContaRequest(DateOnly DataRecebimento, decimal? ValorRecebido = null);
