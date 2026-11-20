// UpdateTaxRuleCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.TaxRules.Common;
using SalariumSphere.Application.Features.TaxRules.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.TaxRules.Commands;

public class UpdateTaxRuleCommandHandler : IRequestHandler<UpdateTaxRuleCommand, TaxRuleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public UpdateTaxRuleCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<TaxRuleDto> Handle(UpdateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var rule = await _context.Set<PayrollTaxRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.TenantId == tenant.Id, cancellationToken);
        if (rule == null)
            throw new Exception($"Tax rule with ID {request.Id} not found.");

        rule.Name = request.Name;
        rule.Description = request.Description;
        rule.CountryCode = request.CountryCode;
        rule.IsMandatory = request.IsMandatory;
        rule.CalculationType = request.CalculationType;
        rule.FixedAmount = request.FixedAmount;
        rule.FixedAmountCurrency = request.FixedAmountCurrency;
        rule.PercentageValue = request.PercentageValue;
        rule.TieredConfigJson = request.TieredConfigJson;
        rule.CapAmount = request.CapAmount;
        rule.CapCurrency = request.CapCurrency;
        rule.ThresholdAmount = request.ThresholdAmount;
        rule.ThresholdCurrency = request.ThresholdCurrency;
        rule.Priority = request.Priority;
        rule.IsActive = request.IsActive;
        rule.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return GetTaxRulesQueryHandler.MapToDto(rule);
    }
}