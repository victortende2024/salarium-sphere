using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using System.Text.Json;

namespace SalariumSphere.Infrastructure.Services;

public class PayrollRunService : IPayrollRunService
{
    private readonly IApplicationDbContext _context;
    private readonly IRulesEngineService _rulesEngineService;

    public PayrollRunService(IApplicationDbContext context, IRulesEngineService rulesEngineService)
    {
        _context = context;
        _rulesEngineService = rulesEngineService;
    }

    public async Task<PayrollRunSummary> RunPayrollAsync(PayrollRunRequest request)
    {
        var summary = new PayrollRunSummary();
        var errors = new List<string>();

        // 1. Fetch all active employees for the company with their profiles and benefits
        var employees = await _context.Set<Employee>()
            .Include(e => e.PrimaryProfile)
                .ThenInclude(p => p.DefaultBenefits)
            .Where(e => e.CompanyId == request.CompanyId && e.IsActive)
            .ToListAsync();

        if (!employees.Any())
            throw new InvalidOperationException("No active employees found for this company.");

        // 2. Create the PayrollRun header
        var payrollRun = new PayrollRun
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            PayPeriodStart = request.PayPeriodStart,
            PayPeriodEnd = request.PayPeriodEnd,
            Status = "Processing",
            TotalEmployees = employees.Count,
            TenantId = employees.First().TenantId
        };

        await _context.Set<PayrollRun>().AddAsync(payrollRun);
        await _context.SaveChangesAsync();

        decimal totalGross = 0;
        decimal totalNet = 0;
        int successfulCount = 0;
        int failedCount = 0;

        // Fetch all active tax rules (global)
        var taxRules = await _context.Set<PayrollTaxRule>()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync();

        // 3. Process each employee
        foreach (var employee in employees)
        {
            // Fetch active salary structures for this employee (USD and ZiG)
            var salaryStructures = await _context.Set<EmployeeSalaryStructure>()
                .Where(s => s.EmployeeId == employee.Id && s.IsActive &&
                            (s.EffectiveEndDate == null || s.EffectiveEndDate > DateTime.UtcNow))
                .ToListAsync();

            if (!salaryStructures.Any())
            {
                // If no salary structure, use the profile's base salary
                var profileSalary = new EmployeeSalaryStructure
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employee.Id,
                    Currency = employee.PrimaryProfile.BaseSalaryCurrency,
                    Amount = employee.PrimaryProfile.BaseSalaryAmount,
                    EffectiveStartDate = DateTime.UtcNow,
                    TenantId = employee.TenantId,
                    IsActive = true
                };
                salaryStructures.Add(profileSalary);
            }

            // Fetch custom benefits for this employee
            var customBenefits = await _context.Set<EmployeeCustomBenefit>()
                .Where(b => b.EmployeeId == employee.Id && b.IsActive)
                .ToListAsync();

            // Fetch employee-specific deductions
            var employeeDeductions = await _context.Set<EmployeeDeduction>()
                .Where(d => d.EmployeeId == employee.Id && d.IsActive)
                .ToListAsync();

            // Process each currency separately
            foreach (var salaryStructure in salaryStructures)
            {
                try
                {
                    var context = new PayrollContext
                    {
                        Employee = employee,
                        Profile = employee.PrimaryProfile,
                        CustomBenefits = customBenefits,
                        SalaryStructures = new List<EmployeeSalaryStructure> { salaryStructure },
                        EmployeeDeductions = employeeDeductions,
                        TaxRules = taxRules
                    };

                    // Execute the rules engine for benefits
                    var result = await _rulesEngineService.ExecuteWorkflowAsync("BenefitsCalculation", context);

                    // Set the correct currency
                    result.Currency = salaryStructure.Currency;

                    // Add the base salary from the salary structure
                    result.GrossPay += salaryStructure.Amount;

                    // Calculate total taxes (mandatory)
                    decimal totalTax = 0;
                    foreach (var taxRule in taxRules)
                    {
                        var taxAmount = DeductionCalculator.CalculateDeduction(
                            grossPay: result.GrossPay,
                            baseCurrency: salaryStructure.Currency,
                            calculationType: taxRule.CalculationType,
                            fixedAmount: taxRule.FixedAmount,
                            percentageValue: taxRule.PercentageValue,
                            tieredConfigJson: taxRule.TieredConfigJson,
                            capAmount: taxRule.CapAmount,
                            thresholdAmount: taxRule.ThresholdAmount
                        );

                        if (taxAmount > 0)
                        {
                            totalTax += taxAmount;
                            result.LineItems.Add(new PayrollLineItem
                            {
                                Name = taxRule.Name,
                                Type = "Tax",
                                Amount = taxAmount,
                                Source = "TaxRule"
                            });
                        }
                    }

                    // Calculate employee deductions (both fixed and percentage)
                    decimal totalDeductions = 0;
                    foreach (var deduction in employeeDeductions)
                    {
                        var deductionAmount = DeductionCalculator.CalculateDeduction(
                            grossPay: result.GrossPay,
                            baseCurrency: salaryStructure.Currency,
                            calculationType: deduction.CalculationType,
                            fixedAmount: deduction.FixedAmount,
                            percentageValue: deduction.PercentageValue,
                            tieredConfigJson: deduction.TieredConfigJson,
                            capAmount: deduction.CapAmount,
                            thresholdAmount: deduction.ThresholdAmount
                        );

                        if (deductionAmount > 0)
                        {
                            totalDeductions += deductionAmount;
                            result.LineItems.Add(new PayrollLineItem
                            {
                                Name = deduction.DeductionName,
                                Type = "Deduction",
                                Amount = deductionAmount,
                                Source = "EmployeeDeduction"
                            });
                        }
                    }

                    // Final net pay
                    result.TotalTax = totalTax;
                    result.TotalDeductions = totalDeductions;
                    result.NetPay = result.GrossPay - totalTax - totalDeductions;

                    // Serialize line items to JSON
                    var lineItemsJson = JsonSerializer.Serialize(result.LineItems);

                    // Create the detail record
                    var detail = new PayrollRunDetail
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunId = payrollRun.Id,
                        EmployeeId = employee.Id,
                        Currency = salaryStructure.Currency,
                        GrossPay = result.GrossPay,
                        TotalDeductions = result.TotalDeductions,
                        NetPay = result.NetPay,
                        LineItemsJson = lineItemsJson,
                        TenantId = employee.TenantId
                    };

                    await _context.Set<PayrollRunDetail>().AddAsync(detail);

                    totalGross += result.GrossPay;
                    totalNet += result.NetPay;
                    successfulCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Employee {employee.EmployeeCode} ({salaryStructure.Currency}): {ex.Message}");
                    failedCount++;
                }
            }
        }

        // 4. Update the PayrollRun header
        payrollRun.TotalGrossPay = totalGross;
        payrollRun.TotalNetPay = totalNet;
        payrollRun.Status = "Completed";
        payrollRun.CompletedAtUtc = DateTime.UtcNow;
        payrollRun.TotalEmployees = employees.Count;

        await _context.SaveChangesAsync();

        // 5. Build the summary
        summary.PayrollRunId = payrollRun.Id;
        summary.TotalEmployees = employees.Count;
        summary.SuccessfulEmployees = successfulCount;
        summary.FailedEmployees = failedCount;
        summary.TotalGrossPay = totalGross;
        summary.TotalNetPay = totalNet;
        summary.Status = payrollRun.Status;
        summary.Errors = errors;

        return summary;
    }
}