using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var department = await _context.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == tenant.Id, cancellationToken);
        if (department == null)
            throw new Exception($"Department with ID {request.Id} not found.");

        // Verify company belongs to tenant
        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId && c.TenantId == tenant.Id, cancellationToken);
        if (company == null)
            throw new Exception("Company not found or does not belong to this tenant.");

        department.Name = request.Name;
        department.CompanyId = request.CompanyId;
        department.IsActive = request.IsActive;
        department.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            CompanyId = department.CompanyId,
            IsActive = department.IsActive
        };
    }
}