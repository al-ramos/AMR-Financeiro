using MediatR;
using Microsoft.AspNetCore.Mvc;
using RDS.Financeiro.Application.Features.PlanoContas.Commands;
using RDS.Financeiro.Application.Features.PlanoContas.Queries;

namespace RDS.Financeiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlanoContasByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/planocontas
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarPlanoContasCommand cmd, CancellationToken ct)
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

    // PUT api/planocontas/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarDescricaoRequest req, CancellationToken ct)
    {
        var ok = await mediator.Send(new AtualizarPlanoContasCommand(id, req.Descricao), ct);
        return ok ? NoContent() : NotFound();
    }

    // PATCH api/planocontas/5/inativar
    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        try
        {
            var ok = await mediator.Send(new InativarPlanoContasCommand(id), ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }

    // PATCH api/planocontas/5/ativar
    [HttpPatch("{id:int}/ativar")]
    public async Task<IActionResult> Ativar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new AtivarPlanoContasCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }
}

public record AtualizarDescricaoRequest(string Descricao);
