using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.ContasBancarias.Commands;
using AMR.Financeiro.Application.Features.ContasBancarias.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/contas-bancarias")]
public class ContasBancariasController(IMediator mediator) : ControllerBase
{
    // GET api/contas-bancarias?incluirInativas=false
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool incluirInativas, CancellationToken ct) =>
        Ok(await mediator.Send(new GetContasBancariasQuery(incluirInativas), ct));

    // GET api/contas-bancarias/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetContaBancariaByIdQuery(id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    // GET api/contas-bancarias/{id}/extrato
    [HttpGet("{id:int}/extrato")]
    public async Task<IActionResult> GetExtrato(int id, CancellationToken ct) =>
        Ok(await mediator.Send(new GetExtratoQuery(id), ct));

    // POST api/contas-bancarias
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarContaBancariaCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return Created($"/api/contas-bancarias/{id}", new { id });
    }

    // PUT api/contas-bancarias/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarContaBancariaCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id) return BadRequest("Id divergente.");
        var ok = await mediator.Send(cmd, ct);
        return ok ? NoContent() : NotFound();
    }

    // DELETE api/contas-bancarias/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        var ok = await mediator.Send(new DesativarContaBancariaCommand(id), ct);
        return ok ? NoContent() : NotFound();
    }
}
