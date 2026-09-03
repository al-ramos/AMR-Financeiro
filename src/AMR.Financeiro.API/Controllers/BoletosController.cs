using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.Boletos.Commands;
using AMR.Financeiro.Application.Features.Boletos.Queries;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoletosController(IMediator mediator) : ControllerBase
{
    // POST api/boletos/gerar
    [HttpPost("gerar")]
    public async Task<IActionResult> Gerar([FromBody] GerarBoletoCommand cmd, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.BoletoId }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // GET api/boletos?cdFilial=1&status=Gerado&banco=Itau
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int cdFilial,
        [FromQuery] StatusBoleto? status,
        [FromQuery] BancoBoleto? banco,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ListBoletosQuery(cdFilial, status, banco), ct);
        return Ok(result);
    }

    // GET api/boletos/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBoletoByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/boletos/remessa
    [HttpPost("remessa")]
    public async Task<IActionResult> GerarRemessa([FromBody] GerarRemessaCommand cmd, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(cmd, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // POST api/boletos/retorno
    [HttpPost("retorno")]
    public async Task<IActionResult> ProcessarRetorno([FromBody] ProcessarRetornoCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return Ok(result);
    }

    // DELETE api/boletos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new CancelarBoletoCommand(id), ct);
            return result.Sucesso ? NoContent() : Conflict(new { erro = result.MensagemErro });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
