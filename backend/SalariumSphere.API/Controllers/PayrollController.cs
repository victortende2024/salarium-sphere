using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using SalariumSphere.Infrastructure.Services;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IRulesEngineService _rulesEngineService;
    private readonly IPayrollRunService _payrollRunService;

    public PayrollController(IApplicationDbContext context, IRulesEngineService rulesEngineService, IPayrollRunService payrollRunService)
    {
        _context = context;
        _rulesEngineService = rulesEngineService;
        _payrollRunService = payrollRunService;
    }

    [HttpGet("test-calculate/{employeeId}")]
    public async Task<IActionResult> TestCalculate(Guid employeeId)
    {
        // 1. Fetch employee with all related data – INCLUDING PrimaryProfile
        var employee = await _context.Set<Employee>()
            .Include(e => e.PrimaryProfile)  // <-- ADD THIS LINE
            .Include(e => e.Company)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
            return NotFound("Employee not found");

        // 2. Fetch custom benefits for this employee
        var customBenefits = await _context.Set<EmployeeCustomBenefit>()
            .Where(b => b.EmployeeId == employeeId && b.IsActive)
            .ToListAsync();

        // 3. Build the PayrollContext
        var context = new PayrollContext
        {
            Employee = employee,
            Profile = employee.PrimaryProfile, // Now this will be populated
            CustomBenefits = customBenefits,
            SalaryStructures = new List<EmployeeSalaryStructure>()
        };

        // 4. Execute the rules engine
        var result = await _rulesEngineService.ExecuteWorkflowAsync("BenefitsCalculation", context);

        return Ok(result);
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunPayroll([FromBody] PayrollRunRequest request)
    {
        try
        {
            var summary = await _payrollRunService.RunPayrollAsync(request);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("runs/{id}")]
    public async Task<IActionResult> GetPayrollRun(Guid id)
    {
        var run = await _context.Set<PayrollRun>()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (run == null)
            return NotFound();
        return Ok(run);
    }

    [HttpGet("runs/{id}/details")]
    public async Task<IActionResult> GetPayrollRunDetails(Guid id)
    {
        var details = await _context.Set<PayrollRunDetail>()
            .Include(d => d.Employee)
            .Where(d => d.PayrollRunId == id)
            .ToListAsync();
        return Ok(details.Select(d => new
        {
            d.EmployeeId,
            d.Employee.FirstName,
            d.Employee.LastName,
            d.Employee.EmployeeCode,
            d.Currency,
            d.GrossPay,
            d.TotalDeductions,
            d.NetPay,
            d.LineItemsJson
        }));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid companyId)
    {
        var runs = await _context.Set<PayrollRun>()
            .Where(r => r.CompanyId == companyId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new
            {
                payrollRunId = r.Id,   
                r.PayPeriodStart,
                r.PayPeriodEnd,
                r.TotalGrossPay,
                r.TotalNetPay,
                r.Status,
                r.TotalEmployees,
                r.CompletedAtUtc
            })
            .ToListAsync();
        return Ok(runs);
    }
}
