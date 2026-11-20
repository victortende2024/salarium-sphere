using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.TaxRules.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.TaxRules.Queries;

public class GetTaxRulesQueryHandler : IRequestHandler<GetTaxRulesQuery, List<TaxRuleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetTaxRulesQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<List<TaxRuleDto>> Handle(GetTaxRulesQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var rules = await _context.Set<PayrollTaxRule>()
            .Where(r => r.TenantId == tenant.Id)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
        return rules.Select(MapToDto).ToList();
    }

    public static TaxRuleDto MapToDto(PayrollTaxRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Description = rule.Description,
        CountryCode = rule.CountryCode,
        IsMandatory = rule.IsMandatory,
        CalculationType = rule.CalculationType,
        FixedAmount = rule.FixedAmount,
        FixedAmountCurrency = rule.FixedAmountCurrency,
        PercentageValue = rule.PercentageValue,
        TieredConfigJson = rule.TieredConfigJson,
        CapAmount = rule.CapAmount,
        CapCurrency = rule.CapCurrency,
        ThresholdAmount = rule.ThresholdAmount,
        ThresholdCurrency = rule.ThresholdCurrency,
        Priority = rule.Priority,
        IsActive = rule.IsActive
    };
}