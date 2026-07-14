using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.NotasFiscais.Commands;
using AMR.Financeiro.Application.Features.NotasFiscais.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NFeController(IMediator mediator) : ControllerBase
{
    // POST api/nfe/emitir
    [HttpPost("emitir")]
    public async Task<IActionResult> Emitir([FromBody] EmitirNFeCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.Sucesso
            ? CreatedAtAction(nameof(GetById), new { id = result.NotaFiscalId }, result)
            : UnprocessableEntity(result);
    }

    // GET api/nfe?cdFilial=1&ano=2026
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int cdFilial, [FromQuery] int? ano, CancellationToken ct)
    {
        var result = await mediator.Send(new ListNFeQuery(cdFilial, ano), ct);
        return Ok(result);
    }

    // GET api/nfe/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetNFeByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // DELETE api/nfe/5/cancelar
    [HttpDelete("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromBody] CancelarNFeRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new CancelarNFeCommand(id, req.Justificativa), ct);
            return result.Sucesso ? NoContent() : Conflict(new { erro = result.MensagemErro });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // GET api/nfe/5/danfe
    [HttpGet("{id:int}/danfe")]
    public async Task<IActionResult> GetDanfe(int id, CancellationToken ct)
    {
        var pdfBase64 = await mediator.Send(new GerarDanfeQuery(id), ct);
        return pdfBase64 is null
            ? NotFound(new { erro = "NF-e não encontrada ou sem XML autorizado." })
            : Ok(new { pdfBase64 });
    }
}

public record CancelarNFeRequest(string Justificativa);
