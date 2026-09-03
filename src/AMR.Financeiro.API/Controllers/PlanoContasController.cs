using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;
using AMR.Financeiro.Application.Features.PlanoDeContas.Queries;

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
        var result = await mediator.Send(new GetContaByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/planocontas
    // Codigo duplicado e hierarquia invalida chegam como InvalidOperationException
    // e o ExceptionHandlingMiddleware as traduz em 422 — por isso nao ha try/catch aqui.
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CriarContaCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // PUT api/planocontas/5
    // Atualiza apenas a descrição: GrupoDRE e OrdemExibicao são relidos da conta
    // atual para que a reclassificação contábil não seja sobrescrita por omissão.
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarDescricaoRequest req, CancellationToken ct)
    {
        var atual = await mediator.Send(new GetContaByIdQuery(id), ct);
        if (atual is null) return NotFound();

        var ok = await mediator.Send(
            new AtualizarContaCommand(id, req.Descricao, atual.GrupoDRE, atual.OrdemExibicao), ct);

        return ok ? NoContent() : NotFound();
    }

    // PATCH api/planocontas/5/inativar
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new InativarContaCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }

    // PATCH api/planocontas/5/ativar
    [HttpPatch("{id:int}/ativar")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new ReativarContaCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }
}

public record AtualizarDescricaoRequest(string Descricao);
