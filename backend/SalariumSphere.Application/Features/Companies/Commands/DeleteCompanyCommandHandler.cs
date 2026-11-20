using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public DeleteCompanyCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenant.Id, cancellationToken);
        if (company == null)
            throw new Exception($"Company with ID {request.Id} not found.");

        // Check if there are employees under this company
        var hasEmployees = await _context.Set<Employee>()
            .AnyAsync(e => e.CompanyId == company.Id && e.IsActive, cancellationToken);
        if (hasEmployees)
            throw new Exception("Cannot delete company with active employees. Move or deactivate employees first.");

        company.IsActive = false;
        company.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}