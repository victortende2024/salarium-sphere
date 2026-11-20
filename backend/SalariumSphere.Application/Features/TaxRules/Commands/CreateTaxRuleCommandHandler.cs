// CreateTaxRuleCommandHandler.cs
using MediatR;
using SalariumSphere.Application.Features.TaxRules.Common;
using SalariumSphere.Application.Features.TaxRules.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.TaxRules.Commands;

public class CreateTaxRuleCommandHandler : IRequestHandler<CreateTaxRuleCommand, TaxRuleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CreateTaxRuleCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<TaxRuleDto> Handle(CreateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var rule = new PayrollTaxRule
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CountryCode = request.CountryCode,
            IsMandatory = request.IsMandatory,
            CalculationType = request.CalculationType,
            FixedAmount = request.FixedAmount,
            FixedAmountCurrency = request.FixedAmountCurrency,
            PercentageValue = request.PercentageValue,
            TieredConfigJson = request.TieredConfigJson,
            CapAmount = request.CapAmount,
            CapCurrency = request.CapCurrency,
            ThresholdAmount = request.ThresholdAmount,
            ThresholdCurrency = request.ThresholdCurrency,
            Priority = request.Priority,
            IsActive = request.IsActive,
            TenantId = tenant.Id
        };

        await _context.Set<PayrollTaxRule>().AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return GetTaxRulesQueryHandler.MapToDto(rule);
    }
}