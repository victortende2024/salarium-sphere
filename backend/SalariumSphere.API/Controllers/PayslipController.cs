using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Interfaces;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayslipController : ControllerBase
{
    private readonly IPayslipService _payslipService;

    public PayslipController(IPayslipService payslipService)
    {
        _payslipService = payslipService;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<List<PayslipDto>>> GetEmployeePayslips(Guid employeeId, [FromQuery] int limit = 12)
    {
        var payslips = await _payslipService.GetPayslipsForEmployeeAsync(employeeId, limit);
        return Ok(payslips);
    }

    [HttpGet("{employeeId}/{payrollRunId}")]
    public async Task<ActionResult<PayslipDto>> GetPayslip(Guid employeeId, Guid payrollRunId)
    {
        var payslip = await _payslipService.GetPayslipAsync(employeeId, payrollRunId);
        return Ok(payslip);
    }

    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<List<PayslipDto>>> GetCompanyPayslips(
        Guid companyId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var payslips = await _payslipService.GetPayslipsForCompanyAsync(companyId, fromDate, toDate);
        return Ok(payslips);
    }

    [HttpGet("{employeeId}/{payrollRunId}/pdf")]
    public async Task<IActionResult> DownloadPayslipPdf(Guid employeeId, Guid payrollRunId)
    {
        var pdfBytes = await _payslipService.GeneratePayslipPdfAsync(employeeId, payrollRunId);
        return File(pdfBytes, "application/pdf", $"payslip_{employeeId}_{payrollRunId}.pdf");
    }
}