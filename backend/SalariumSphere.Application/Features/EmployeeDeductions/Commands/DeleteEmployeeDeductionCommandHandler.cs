using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Commands;

public class DeleteEmployeeDeductionCommandHandler : IRequestHandler<DeleteEmployeeDeductionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public DeleteEmployeeDeductionCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(DeleteEmployeeDeductionCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var ded = await _context.Set<EmployeeDeduction>()
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.TenantId == tenant.Id, cancellationToken);
        if (ded == null)
            throw new Exception($"Deduction with ID {request.Id} not found.");

        ded.IsActive = false;
        ded.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}