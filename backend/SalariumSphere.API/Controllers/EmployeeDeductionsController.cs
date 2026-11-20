using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalariumSphere.Application.Features.EmployeeDeductions.Commands;
using SalariumSphere.Application.Features.EmployeeDeductions.Queries;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/employeedeductions")]
[Authorize(Roles = "Admin,PayrollManager")]
public class EmployeeDeductionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeeDeductionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId)
        => Ok(await _mediator.Send(new GetEmployeeDeductionsQuery { EmployeeId = employeeId }));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await _mediator.Send(new GetEmployeeDeductionByIdQuery { Id = id }));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDeductionCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDeductionCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteEmployeeDeductionCommand { Id = id });
        return NoContent(); // 204 No Content – standard for DELETE
    }
}