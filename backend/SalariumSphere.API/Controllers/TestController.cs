using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires a valid JWT
public class TestController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public TestController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _context.Set<Company>().ToListAsync();
        return Ok(companies);
    }
}
