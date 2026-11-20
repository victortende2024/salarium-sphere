using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Users.Common;
using SalariumSphere.Application.Features.Users.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public UpdateUserCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.TenantId == tenant.Id, cancellationToken);
        if (user == null)
            throw new Exception($"User with ID {request.Id} not found.");

        // Check email uniqueness (excluding self)
        var existing = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email && u.Id != request.Id && u.TenantId == tenant.Id, cancellationToken);
        if (existing)
            throw new Exception("Another user with this email already exists.");

        // Verify employee exists if provided
        if (request.EmployeeId.HasValue)
        {
            var employee = await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId.Value && e.TenantId == tenant.Id, cancellationToken);
            if (employee == null)
                throw new Exception("Employee not found or does not belong to this tenant.");
        }

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Role = request.Role;
        user.EmployeeId = request.EmployeeId;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Re-fetch with Employee included
        var updated = await _context.Set<User>()
            .Include(u => u.Employee)
            .FirstAsync(u => u.Id == user.Id, cancellationToken);

        return GetUsersQueryHandler.MapToDto(updated);
    }
}