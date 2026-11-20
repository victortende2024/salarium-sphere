// GetTaxRuleByIdQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.TaxRules.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.TaxRules.Queries;

public class GetTaxRuleByIdQueryHandler : IRequestHandler<GetTaxRuleByIdQuery, TaxRuleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetTaxRuleByIdQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<TaxRuleDto> Handle(GetTaxRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var rule = await _context.Set<PayrollTaxRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.TenantId == tenant.Id, cancellationToken);
        if (rule == null)
            throw new Exception($"Tax rule with ID {request.Id} not found.");
        return GetTaxRulesQueryHandler.MapToDto(rule);
    }
}