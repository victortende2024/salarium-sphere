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
public class CompaniesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;       

    public CompaniesController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    //[HttpGet]
    //public async Task<IActionResult> GetCompanies()
    //{
    //    var companies = await _context.Set<Company>()
    //        .Where(c => c.IsActive)
    //        .OrderBy(c => c.Name)
    //        .ToListAsync();
    //    return Ok(companies);
    //}
    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll() => Ok(await _mediator.Send(new GetCompaniesQuery()));

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> Get(Guid id) => Ok(await _mediator.Send(new GetCompanyByIdQuery { Id = id }));

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyCommand command) => Ok(await _mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, [FromBody] UpdateCompanyCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCompanyCommand { Id = id });
        return NoContent();
    }

}