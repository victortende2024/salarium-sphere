using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CreateDepartmentCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId && c.TenantId == tenant.Id, cancellationToken);
        if (company == null)
            throw new Exception("Company not found or does not belong to this tenant.");

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CompanyId = request.CompanyId,
            TenantId = tenant.Id,
            IsActive = true
        };

        await _context.Set<Department>().AddAsync(department, cancellationToken);
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