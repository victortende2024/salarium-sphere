using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Employees.Commands;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public DeleteEmployeeCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.TenantId == tenant.Id, cancellationToken);

        if (employee == null)
            throw new Exception($"Employee with ID {request.Id} not found.");

        // Soft delete
        employee.IsActive = false;
        employee.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}