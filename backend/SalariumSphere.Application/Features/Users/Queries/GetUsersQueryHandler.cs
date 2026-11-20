using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Users.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Users.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetUsersQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var users = await _context.Set<User>()
            .Include(u => u.Employee)
            .Where(u => u.TenantId == tenant.Id)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return users.Select(MapToDto).ToList();
    }

    public static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            EmployeeId = user.EmployeeId,
            EmployeeName = user.Employee != null ? $"{user.Employee.FirstName} {user.Employee.LastName}" : null,
            EmployeeCode = user.Employee?.EmployeeCode,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        };
    }
}