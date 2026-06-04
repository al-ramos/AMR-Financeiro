using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.PlanoContas.Commands;
using AMR.Financeiro.Application.Features.PlanoContas.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class PlanoContasController(IMediator mediator) : ControllerBase
{
    // GET api/planocontas?cdFilial=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlanoContasQuery(cdFilial), ct);
        return Ok(result);
    }

    // GET api/planocontas/arvore?cdFilial=1
    [HttpGet("arvore")]
    public async Task<IActionResult> GetArvore([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlanoContasArvoreQuery(cdFilial), ct);
        return Ok(result);
    }

    // GET api/planocontas/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlanoContasByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/planocontas
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CriarPlanoContasCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // PUT api/planocontas/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarDescricaoRequest req, CancellationToken ct)
    {
        var ok = await mediator.Send(new AtualizarPlanoContasCommand(id, req.Descricao), ct);
        return ok ? NoContent() : NotFound();
    }

    // PATCH api/planocontas/5/inativar
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new InativarPlanoContasCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }

    // PATCH api/planocontas/5/ativar
    [HttpPatch("{id:int}/ativar")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new AtivarPlanoContasCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }
}

public record AtualizarDescricaoRequest(string Descricao);
