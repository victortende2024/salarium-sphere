using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using System.Text.Json;

namespace SalariumSphere.Infrastructure.Services;

public class RulesEngineService : IRulesEngineService
{
    private readonly IApplicationDbContext _context;

    public RulesEngineService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollResult> ExecuteWorkflowAsync(string workflowName, PayrollContext context)
    {
        // 1. Fetch active rules
        var ruleEntities = await _context.Set<PayrollRule>()
            .Where(r => r.WorkflowName == workflowName && r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync();

        if (!ruleEntities.Any())
        {
            throw new InvalidOperationException($"No rules found for workflow '{workflowName}'.");
        }

        // 2. Deserialize rules
        var rules = new List<Rule>();
        foreach (var entity in ruleEntities)
        {
            var rule = JsonSerializer.Deserialize<Rule>(entity.RuleJson);
            if (rule != null)
                rules.Add(rule);
        }

        var workflow = new Workflow
        {
            WorkflowName = workflowName,
            Rules = rules
        };

        var engine = new RulesEngine.RulesEngine(new[] { workflow });

        // 3. Execute the workflow
        var results = await engine.ExecuteAllRulesAsync(workflowName, context);

        // 4. Build PayrollResult
        var payrollResult = new PayrollResult
        {
            EmployeeId = context.Employee.Id,
            Currency = context.SalaryStructures.FirstOrDefault()?.Currency ?? Domain.Enums.CurrencyCode.USD
        };

        decimal gross = 0;
        decimal deductions = 0;

        // Add profile default benefits first (base)
        foreach (var benefit in context.Profile.DefaultBenefits
            .Where(b => b.IsActive && b.Currency == payrollResult.Currency))
        {
            gross += benefit.Amount;
            payrollResult.LineItems.Add(new PayrollLineItem
            {
                Name = benefit.BenefitName,
                Type = "Earning",
                Amount = benefit.Amount,
                Source = "ProfileDefault"
            });
        }

        foreach (var ruleResult in results)
        {
            // --- DIAGNOSTIC ENTRY: Log the rule status ---
            var lineItem = new PayrollLineItem
            {
                Name = ruleResult.Rule.RuleName,
                Type = "Diagnostic",
                Amount = 0,
                Source = "Engine"
            };

            if (!ruleResult.IsSuccess)
            {
                // Condition failed – log why
                lineItem.Name = $"{ruleResult.Rule.RuleName} (FAILED)";
                lineItem.Type = "Error";
                lineItem.Amount = 0;
                lineItem.Source = ruleResult.ExceptionMessage ?? "Condition evaluated to false";
                payrollResult.LineItems.Add(lineItem);
                continue;
            }

            // Condition passed – now check for output
            if (ruleResult.ActionResult != null && ruleResult.ActionResult.Output != null)
            {
                var outputValue = ruleResult.ActionResult.Output;
                decimal amount = 0;

                // Try to extract the numeric output
                if (outputValue is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        amount = jsonElement.GetDecimal();
                    }
                    else if (jsonElement.ValueKind == JsonValueKind.String)
                    {
                        decimal.TryParse(jsonElement.GetString(), out amount);
                    }
                }
                else
                {
                    decimal.TryParse(outputValue.ToString(), out amount);
                }

                if (amount != 0)
                {
                    if (amount >= 0)
                    {
                        gross += amount;
                        payrollResult.LineItems.Add(new PayrollLineItem
                        {
                            Name = ruleResult.Rule.RuleName,
                            Type = "Earning",
                            Amount = amount,
                            Source = "Rule"
                        });
                    }
                    else
                    {
                        deductions += Math.Abs(amount);
                        payrollResult.LineItems.Add(new PayrollLineItem
                        {
                            Name = ruleResult.Rule.RuleName,
                            Type = "Deduction",
                            Amount = amount,
                            Source = "Rule"
                        });
                    }
                }
                else
                {
                    // Rule succeeded but output was 0 or couldn't be parsed
                    payrollResult.LineItems.Add(new PayrollLineItem
                    {
                        Name = $"{ruleResult.Rule.RuleName} (Output Zero/Invalid)",
                        Type = "Warning",
                        Amount = 0,
                        Source = $"Raw Output: {outputValue}"
                    });
                }
            }
            else
            {
                // Rule succeeded but ActionResult or Output is null
                payrollResult.LineItems.Add(new PayrollLineItem
                {
                    Name = $"{ruleResult.Rule.RuleName} (No Output)",
                    Type = "Warning",
                    Amount = 0,
                    Source = "ActionResult or Output was null"
                });
            }
        }

        payrollResult.GrossPay = gross;
        payrollResult.TotalDeductions = deductions;
        payrollResult.NetPay = gross - deductions;

        return payrollResult;
    }
}