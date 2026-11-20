// DeleteTaxRuleCommandHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.TaxRules.Commands;

public class DeleteTaxRuleCommandHandler : IRequestHandler<DeleteTaxRuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public DeleteTaxRuleCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(DeleteTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var rule = await _context.Set<PayrollTaxRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.TenantId == tenant.Id, cancellationToken);
        if (rule == null)
            throw new Exception($"Tax rule with ID {request.Id} not found.");

        rule.IsActive = false;
        rule.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}