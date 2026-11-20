using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalariumSphere.Application.Features.TaxRules.Commands;
using SalariumSphere.Application.Features.TaxRules.Queries;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/taxrules")]
[Authorize(Roles = "Admin")] // Only Admins can manage tax rules
public class TaxRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxRulesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _mediator.Send(new GetTaxRulesQuery()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id) => Ok(await _mediator.Send(new GetTaxRuleByIdQuery { Id = id }));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaxRuleCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRuleCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTaxRuleCommand { Id = id });
        return NoContent();
    }
}