using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public DeleteDepartmentCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var department = await _context.Set<Department>()
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == tenant.Id, cancellationToken);
        if (department == null)
            throw new Exception($"Department with ID {request.Id} not found.");

        // Check if there are employees under this department
        var hasEmployees = await _context.Set<Employee>()
            .AnyAsync(e => e.DepartmentId == department.Id && e.IsActive, cancellationToken);
        if (hasEmployees)
            throw new Exception("Cannot delete department with active employees. Move or deactivate employees first.");

        department.IsActive = false;
        department.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}