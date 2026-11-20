using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Users.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetUserByIdQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var user = await _context.Set<User>()
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.TenantId == tenant.Id, cancellationToken);
        if (user == null)
            throw new Exception($"User with ID {request.Id} not found.");

        return GetUsersQueryHandler.MapToDto(user);
    }
}