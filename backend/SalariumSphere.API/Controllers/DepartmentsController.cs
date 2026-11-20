using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Commands;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Features.Companies.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public DepartmentsController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    //[HttpGet]
    //public async Task<IActionResult> GetDepartments([FromQuery] Guid? companyId)
    //{
    //    var query = _context.Set<Department>().Where(d => d.IsActive);
    //    if (companyId.HasValue)
    //        query = query.Where(d => d.CompanyId == companyId.Value);
    //    var departments = await query.OrderBy(d => d.Name).ToListAsync();
    //    return Ok(departments);
    //}
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetByCompany([FromQuery] Guid companyId)
    {
        // This uses the existing query; we can add a new query or reuse.
        var companies = await _mediator.Send(new GetCompaniesQuery());
        var company = companies.FirstOrDefault(c => c.Id == companyId);
        return Ok(company?.Departments ?? new List<DepartmentDto>());
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentCommand command) => Ok(await _mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<ActionResult<DepartmentDto>> Update(Guid id, [FromBody] UpdateDepartmentCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDepartmentCommand { Id = id });
        return NoContent();
    }
}