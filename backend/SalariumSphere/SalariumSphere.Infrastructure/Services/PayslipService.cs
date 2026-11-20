using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using System.Text.Json;
using SalariumSphere.Infrastructure.Services; // if PayslipPdfGenerator is in the same namespace

namespace SalariumSphere.Infrastructure.Services;

public class PayslipService : IPayslipService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public PayslipService(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<PayslipDto> GetPayslipAsync(Guid employeeId, Guid payrollRunId)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var detail = await _context.Set<PayrollRunDetail>()
            .Include(d => d.PayrollRun)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Company)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Department)
            .Include(d => d.Employee)
                .ThenInclude(e => e.PrimaryProfile)
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId
                && d.PayrollRunId == payrollRunId);
               // && d.TenantId == tenant.Id);

        if (detail == null)
            throw new Exception("Payslip not found.");

        // Parse line items
        List<PayslipLineItemDto> lineItems = new();
        try
        {
            if (!string.IsNullOrEmpty(detail.LineItemsJson))
            {
                var items = JsonSerializer.Deserialize<List<PayrollLineItem>>(detail.LineItemsJson);
                if (items != null)
                {
                    lineItems = items.Select(i => new PayslipLineItemDto
                    {
                        Name = i.Name,
                        Type = i.Type,
                        Amount = i.Amount,
                        Source = i.Source
                    }).ToList();
                }
            }
        }
        catch { /* Ignore parsing errors */ }

        // Calculate taxes and deductions from line items
        var totalTax = lineItems.Where(i => i.Type == "Tax").Sum(i => i.Amount);
        var totalDeductions = lineItems.Where(i => i.Type == "Deduction").Sum(i => i.Amount);

        return new PayslipDto
        {
            EmployeeId = detail.EmployeeId,
            PayrollRunId = detail.PayrollRunId,
            
            EmployeeName = $"{detail.Employee.FirstName} {detail.Employee.LastName}",
            EmployeeCode = detail.Employee.EmployeeCode,
            Email = detail.Employee.Email,
            CompanyName = detail.Employee.Company?.Name ?? string.Empty,
            DepartmentName = detail.Employee.Department?.Name ?? string.Empty,
            ProfileName = detail.Employee.PrimaryProfile?.Name ?? string.Empty,
            PayPeriod = $"{detail.PayrollRun.PayPeriodStart:MMM dd, yyyy} - {detail.PayrollRun.PayPeriodEnd:MMM dd, yyyy}",
            PayPeriodStart = detail.PayrollRun.PayPeriodStart,
            PayPeriodEnd = detail.PayrollRun.PayPeriodEnd,
            Currency = detail.Currency,
            GrossPay = detail.GrossPay,
            TotalTax = totalTax,
            TotalDeductions = totalDeductions,
            NetPay = detail.NetPay,
            LineItems = lineItems,
            
            GeneratedAt = DateTime.UtcNow,
        };
    }

    public async Task<List<PayslipDto>> GetPayslipsForEmployeeAsync(Guid employeeId, int limit = 12)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var details = await _context.Set<PayrollRunDetail>()
            .Include(d => d.PayrollRun)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Company)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Department)
            .Include(d => d.Employee)
                .ThenInclude(e => e.PrimaryProfile)
            .Where(d => d.EmployeeId == employeeId && d.TenantId == tenant.Id)
            .OrderByDescending(d => d.PayrollRun.PayPeriodEnd)
            .Take(limit)
            .ToListAsync();

        return details.Select(MapToDto).ToList();
    }

    public async Task<List<PayslipDto>> GetPayslipsForCompanyAsync(Guid companyId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var query = _context.Set<PayrollRunDetail>()
            .Include(d => d.PayrollRun)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Company)
            .Include(d => d.Employee)
                .ThenInclude(e => e.Department)
            .Include(d => d.Employee)
                .ThenInclude(e => e.PrimaryProfile)
            .Where(d => d.TenantId == tenant.Id && d.Employee.CompanyId == companyId);

        if (fromDate.HasValue)
            query = query.Where(d => d.PayrollRun.PayPeriodStart >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(d => d.PayrollRun.PayPeriodEnd <= toDate.Value);

        var details = await query
            .OrderByDescending(d => d.PayrollRun.PayPeriodEnd)
            .ToListAsync();

        return details.Select(MapToDto).ToList();
    }

    public async Task<byte[]> GeneratePayslipPdfAsync(Guid employeeId, Guid payrollRunId)
    {
        var payslip = await GetPayslipAsync(employeeId, payrollRunId);
        return PayslipPdfGenerator.GeneratePayslipPdf(payslip);
    }

    private PayslipDto MapToDto(PayrollRunDetail detail)
    {
        List<PayslipLineItemDto> lineItems = new();
        try
        {
            if (!string.IsNullOrEmpty(detail.LineItemsJson))
            {
                var items = JsonSerializer.Deserialize<List<PayrollLineItem>>(detail.LineItemsJson);
                if (items != null)
                {
                    lineItems = items.Select(i => new PayslipLineItemDto
                    {
                        Name = i.Name,
                        Type = i.Type,
                        Amount = i.Amount,
                        Source = i.Source
                    }).ToList();
                }
            }
        }
        catch { /* Ignore */ }

        var totalTax = lineItems.Where(i => i.Type == "Tax").Sum(i => i.Amount);
        var totalDeductions = lineItems.Where(i => i.Type == "Deduction").Sum(i => i.Amount);

        return new PayslipDto
        {
            EmployeeId = detail.EmployeeId,
            PayrollRunId = detail.PayrollRunId,
            EmployeeName = $"{detail.Employee.FirstName} {detail.Employee.LastName}",
            EmployeeCode = detail.Employee.EmployeeCode,
            Email = detail.Employee.Email,
            CompanyName = detail.Employee.Company?.Name ?? string.Empty,
            DepartmentName = detail.Employee.Department?.Name ?? string.Empty,
            ProfileName = detail.Employee.PrimaryProfile?.Name ?? string.Empty,
            PayPeriod = $"{detail.PayrollRun.PayPeriodStart:MMM dd, yyyy} - {detail.PayrollRun.PayPeriodEnd:MMM dd, yyyy}",
            PayPeriodStart = detail.PayrollRun.PayPeriodStart,
            PayPeriodEnd = detail.PayrollRun.PayPeriodEnd,
            Currency = detail.Currency,
            GrossPay = detail.GrossPay,
            TotalTax = totalTax,
            TotalDeductions = totalDeductions,
            NetPay = detail.NetPay,
            LineItems = lineItems,
            GeneratedAt = DateTime.UtcNow
        };
    }
}